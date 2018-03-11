using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Common.Events
{
    /// <summary>
    /// 消息通信渠道 (消息总线)
    /// 具有消息订阅的功能，还具有消息派发的能力
    /// </summary>
    public interface IEventBus : IEventPublisher, IEventSubscriber
    {
    }
}
