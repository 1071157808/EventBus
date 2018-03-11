using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Common.Events
{
    /// <summary>
    /// 事件消息 (数据)
    /// </summary>
    public interface IEvent
    {
        Guid Id { get; }

        DateTime Timestamp { get; }
    }
}
