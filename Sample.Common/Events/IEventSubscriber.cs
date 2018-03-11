using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Common.Events
{
    /// <summary>
    /// 事件订阅器
    /// </summary>
    public interface IEventSubscriber : IDisposable
    {
        //void Subscribe();

        /// <summary>
        /// 事件订阅
        /// </summary>
        void Subscribe<TEvent, TEventHandler>()
            where TEvent : IEvent
            where TEventHandler : IEventHandler<TEvent>;
    }
}
