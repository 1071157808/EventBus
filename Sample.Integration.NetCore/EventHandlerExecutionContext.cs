using Microsoft.Extensions.DependencyInjection;
using Sample.Common;
using Sample.Common.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Integration.NetCore
{
    /**
     * 解决：由于构造函数注入，使得对象之间产生了依赖关系
     * 事件处理器执行上下文(三个方法)：注册事件 是否注册 事件处理
     * 
     * Microsoft.Extensions.DependencyInjection 框架2.0版本之前，
     * IServiceCollection.BuildServiceProvider 方法的返回类型是IServiceProvider，
     * 但从2.0开始，它的返回类型已经从IServiceProvider接口，变成了ServiceProvider类。
     * 这里引出了框架设计的另一个原则，就是依赖较低版本的.NET Core，以便获得更好的兼容性。
     */

    public class EventHandlerExecutionContext : IEventHandlerExecutionContext
    {
        private readonly IServiceCollection registry;
        private readonly Func<IServiceCollection, IServiceProvider> serviceProviderFactory;
        private readonly ConcurrentDictionary<Type, List<Type>> registrations = new ConcurrentDictionary<Type, List<Type>>();

        public EventHandlerExecutionContext(IServiceCollection registry,
            Func<IServiceCollection, IServiceProvider> serviceProviderFactory = null)
        {
            this.registry = registry;
            this.serviceProviderFactory = serviceProviderFactory ?? (sc => registry.BuildServiceProvider());
        }

        public async Task HandleEventAsync(IEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            var eventType = @event.GetType();
            if (this.registrations.TryGetValue(eventType, out List<Type> handlerTypes) &&
                handlerTypes?.Count > 0)
            {
                var serviceProvider = serviceProviderFactory(registry);
                using (var childScope = serviceProvider.CreateScope())
                {
                    foreach (var handlerType in handlerTypes)
                    {
                        var handler = (IEventHandler)childScope.ServiceProvider.GetService(handlerType);
                        if (handler.CanHandle(@event))
                        {
                            await handler.HandleAsync(@event, cancellationToken);
                        }
                    }
                }
            }
        }

        public bool HandlerRegistered<TEvent, THandler>()
            where TEvent : IEvent
            where THandler : IEventHandler<TEvent>
            => HandlerRegistered(typeof(TEvent), typeof(THandler));

        public bool HandlerRegistered(Type eventType, Type handlerType)
        {
            if (this.registrations.TryGetValue(eventType, out List<Type> handlerTypeList))
            {
                return handlerTypeList != null && handlerTypeList.Contains(handlerType);
            }

            return false;
        }

        public void RegisterHandler<TEvent, THandler>()
            where TEvent : IEvent
            where THandler : IEventHandler<TEvent>
            => RegisterHandler(typeof(TEvent), typeof(THandler));

        public void RegisterHandler(Type eventType, Type handlerType)
        {
            Utils.ConcurrentDictionarySafeRegister(eventType, handlerType, registrations);

            //将事件处理 注册为瞬时!!
            registry.AddTransient(handlerType);
        }
    }
}
