using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Common.Events
{
    /// <summary>
    /// 事件处理器
    /// </summary>
    public interface IEventHandler
    {
        /// <summary>
        /// 事件处理
        /// </summary>
        /// <param name="event"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> HandleAsync(IEvent @event, CancellationToken cancellationToken = default);

        /// <summary>
        /// 是否可被当前处理器所处理
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        bool CanHandle(IEvent @event);
    }

    //<in T> 逆变
    //如果某个返回的类型可以由其基类替换，那么这个类型就是支持协变的 狗被动画替换
    //如果某个参数类型可以由其派生类替换，那么这个类型就是支持逆变的 反之!

    //default c#7.1
    /// <summary>
    /// 事件处理
    /// </summary>
    /// <param name="event"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public interface IEventHandler<in T> : IEventHandler
        where T : IEvent
    {
        /// <summary>
        /// 事件处理过程
        /// </summary>
        /// <param name="event"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> HandleAsync(T @event, CancellationToken cancellationToken = default);
    }
}
