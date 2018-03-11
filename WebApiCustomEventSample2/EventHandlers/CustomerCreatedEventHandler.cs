using Microsoft.Extensions.Logging;
using Sample.Common.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApiCustomEventSample2.Events;

namespace WebApiCustomEventSample2.EventHandlers
{
    public class CustomerCreatedEventHandler : IEventHandler<CustomerCreatedEvent>
    {
        private readonly IEventStore eventStore; //也是瞬时的注入
        private readonly ILogger logger;

        public CustomerCreatedEventHandler(IEventStore eventStore, ILogger<CustomerCreatedEventHandler> logger)
        {
            this.eventStore = eventStore;
            this.logger = logger;
            this.logger.LogInformation($"CustomerCreatedEventHandler 构造函数调用完成。Hash Code: {this.GetHashCode()}.");
        }

        public bool CanHandle(IEvent @event)
            => @event.GetType().Equals(typeof(CustomerCreatedEvent));

        public async Task<bool> HandleAsync(CustomerCreatedEvent @event, CancellationToken cancellationToken = default)
        {
            logger.LogInformation($"开始处理 CustomerCreatedEvent 事件，处理器 Hash Code：{this.GetHashCode()}.");
            await eventStore.SaveEventAsync(@event);
            logger.LogInformation($"结束处理 CustomerCreatedEvent 事件，处理器 Hash Code：{this.GetHashCode()}.");

            return true;
        }

        public Task<bool> HandleAsync(IEvent @event, CancellationToken cancellationToken = default)
            => CanHandle(@event) ? HandleAsync((CustomerCreatedEvent)@event, cancellationToken) : Task.FromResult(false);
    }
}
