using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Common.Events
{
    /// <summary>
    /// 事件处理器执行上下文(EHEC)
    /// 为事件处理器提供了一个完整的生命周期管理机制
    /// </summary>
    public interface IEventHandlerExecutionContext
    {
        /// <summary>
        /// 注册事件处理器
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="THandler"></typeparam>
        void RegisterHandler<TEvent, THandler>()
            where TEvent : IEvent
            where THandler : IEventHandler<TEvent>;

        /// <summary>
        /// 注册事件处理器
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="handlerType"></param>
        void RegisterHandler(Type eventType, Type handlerType);

        /// <summary>
        /// 判断事件处理器是否已经注册
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="THandler"></typeparam>
        /// <returns></returns>
        bool HandlerRegistered<TEvent, THandler>()
            where TEvent : IEvent
            where THandler : IEventHandler<TEvent>;

        /// <summary>
        /// 判断事件处理器是否已经注册
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="handlerType"></param>
        /// <returns></returns>
        bool HandlerRegistered(Type eventType, Type handlerType);

        /// <summary>
        /// 处理接收的事件
        /// </summary>
        /// <param name="event"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task HandleEventAsync(IEvent @event, CancellationToken cancellationToken = default);
    }
}
