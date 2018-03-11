using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Common.Events
{
    /// <summary>
    /// 事件处理-事件参数
    /// </summary>
    public class EventProcessedEventArgs : EventArgs
    {
        public EventProcessedEventArgs(IEvent @event)
        {
            this.Event = @event;
        }

        public IEvent Event { get; }
    }
}
