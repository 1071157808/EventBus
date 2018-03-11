using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sample.Common.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.EventBus.RabbitMQ
{
    public class RabbitMQEventBus : BaseEventBus
    {
        private readonly IConnectionFactory connectionFactory;
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string exchangeName;
        private readonly string exchangeType;
        private readonly string queueName;
        private readonly bool autoAck;
        private readonly ILogger logger;
        private bool disposed;

        public RabbitMQEventBus(IConnectionFactory connectionFactory,
            ILogger<RabbitMQEventBus> logger,
            IEventHandlerExecutionContext context,
            string exchangeName,
            string exchangeType = ExchangeType.Fanout,
            string queueName = null,
            bool autoAck = false)
            : base(context)
        {
            this.logger = logger;
            //1.实例化连接工厂
            this.connectionFactory = connectionFactory;
            //2.建立连接
            this.connection = this.connectionFactory.CreateConnection();
            //3.创建信道
            this.channel = this.connection.CreateModel();
            this.exchangeType = exchangeType;
            this.exchangeName = exchangeName;
            this.autoAck = autoAck;
            //4.创建 exchangeType 类型的交换机（exchange）
            this.channel.ExchangeDeclare(this.exchangeName, this.exchangeType);

            this.queueName = this.InitializeEventConsumer(queueName);

            logger.LogInformation($"RabbitMQEventBus构造函数调用完成。Hash Code：{this.GetHashCode()}.");
        }

        private string InitializeEventConsumer(string queue)
        {
            var localQueueName = queue;
            if (string.IsNullOrEmpty(localQueueName))
            {
                //5.创建随机队列名称 断开会自动删除该队列
                localQueueName = channel.QueueDeclare().QueueName;
            }
            else
            {
                //5.申明队列
                this.channel.QueueDeclare(localQueueName, true, false, false, null);
            }
            //6.构造消费者实例
            var consumer = new EventingBasicConsumer(channel);
            //7.绑定消息接收后的事件委托
            consumer.Received += async (model, eventArgument) =>
            {
                var eventBody = eventArgument.Body;
                var json = Encoding.UTF8.GetString(eventBody);
                var @event = (IEvent)JsonConvert.DeserializeObject(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                //处理事件
                await this.eventHandlerExecutionContext.HandleEventAsync(@event);
                if (!autoAck)
                {
                    //7.1 发送消息确认信号（手动消息确认）
                    channel.BasicAck(eventArgument.DeliveryTag, false);
                }
            };
            //8. 启动消费者
            channel.BasicConsume(localQueueName, autoAck: this.autoAck, consumer: consumer);

            return localQueueName;
        }

        public override Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            //All 使得序列化的JSON字符串中带有类型名称信息 （必须的！）
            var json = JsonConvert.SerializeObject(@event, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
            var eventBody = Encoding.UTF8.GetBytes(json);

            //发送数据包 路由键：FullName
            channel.BasicPublish(exchangeName,
                                 @event.GetType().FullName,
                                 null,
                                 eventBody);
            return Task.CompletedTask;
        }

        public override void Subscribe<TEvent, TEventHandler>()
        {
            if (!this.eventHandlerExecutionContext.HandlerRegistered<TEvent, TEventHandler>())
            {
                eventHandlerExecutionContext.RegisterHandler<TEvent, TEventHandler>();

                //绑定队列到指定fanout类型exchange 路由键：FullName
                channel.QueueBind(queueName, exchangeName, typeof(TEvent).FullName);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    channel.Dispose();
                    connection.Dispose();

                    logger.LogInformation($"RabbitMQEventBus已经被Dispose。Hash Code:{this.GetHashCode()}.");
                }

                disposed = true;
                base.Dispose(disposing);
            }
        }
    }
}
