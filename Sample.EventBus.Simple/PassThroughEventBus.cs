using Microsoft.Extensions.Logging;
using Sample.Common.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.EventBus.Simple
{
    /**
     * Note：
     *  为什么PassThroughEventBus可以作为单例注册到IoC容器中？因为它提供了无状态的全局性的基础结构层服务：
     *  事件总线。在PassThroughEventBus的实现中，这种全局性体现得不明显，
     *  我们当然可以每一次HTTP请求都创建一个新的PassThroughEventBus来转发事件消息并作处理。然而，
     *  在今后我们要实现的基于RabbitMQ的事件总线中，如果我们还是每次HTTP请求都创建一个新的消息队列，
     *  不仅性能得不到保证，而且消息并不能路由到新创建的channel上。注意：我们将其注册成单例，
     *  一个很重要的依据是由于它是无状态的，但即使如此，我们也要注意在应用程序退出的时候，
     *  合理Dispose掉它所占用的资源。当然，在这里，ASP.NET Core的IoC机制会帮我们解决这个问题
     *  （因为我注册了PassThroughEventBus，但我没有显式调用Dispose方法，
     *  我仍然能从日志中看到“PassThroughEventBus已经被Dispose”的字样），
     *  然而有些情况下，ASP.NET Core不会帮我们做这些，就需要我们自己手工完成。
     */

    /// <summary>
    /// 当有消息被派发到消息总线时，消息总线将不做任何处理与路由，而是直接将消息推送到订阅方
    /// </summary>
    public class PassThroughEventBus : BaseEventBus
    {
        private readonly EventQueue eventQueue = new EventQueue();
        //private readonly IEnumerable<IEventHandler> eventHandlers;
        private readonly ILogger logger;
        //private readonly IEventHandlerExecutionContext context;

        /**
		 * 因为PassThroughEventBus和CustomerCreatedEventHandler都被注册到了IoC容器中。
		 * 当PassThroughEventBus被resolve的时候，IoC容器会同时resolve它的依赖类型，
		 * 并将resolve的实例注射到构造函数中。所有这些事情都是IoC容器完成的。
		 */
        public PassThroughEventBus(IEventHandlerExecutionContext context, ILogger<PassThroughEventBus> logger)
            :base(context)
        {
            //this.eventHandlers = eventHandlers;
            //this.context = context;
            this.logger = logger;
            logger.LogInformation($"PassThroughEventBus 构造函数调用完成。Hash Code：{this.GetHashCode()}.");

            eventQueue.EventPushed += EventQueue_EventPushed;
        }

        //将传入的事件消息转发到EventQueue上
        public override Task PublishAsync<TEvent>(TEvent @event,
            CancellationToken cancellationToken = default)
            => Task.Factory.StartNew(() => eventQueue.Push(@event));

        public override void Subscribe<TEvent, TEventHandler>()
        {
            //eventQueue.EventPushed += EventQueue_EventPushed;

            if (!eventHandlerExecutionContext.HandlerRegistered<TEvent, TEventHandler>())
            {
                //将事件处理 注册为瞬时的
                eventHandlerExecutionContext.RegisterHandler<TEvent, TEventHandler>();
            }
        }

        private async void EventQueue_EventPushed(object sender, EventProcessedEventArgs e)
        {
            //var ehlist = from eh in eventHandlers
            //             where eh.CanHandle(e.Event)
            //             select eh;

            //ehlist.ToList()
            //   .ForEach(async eh => await eh.HandleAsync(e.Event));

            await eventHandlerExecutionContext.HandleEventAsync(e.Event);
        }

        #region Dispose
        private bool disposedValue;
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.eventQueue.EventPushed -= EventQueue_EventPushed;
                    logger.LogInformation($"PassThroughEventBus 已经被 Dispose。Hash Code:{this.GetHashCode()}.");
                }

                disposedValue = true;
            }
        }
        #endregion
    }
}
