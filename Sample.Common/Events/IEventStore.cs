using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Common.Events
{
    /**
     * Event Store是CQRS体系结构模式中最为重要的一个组成部分。
     * 主要职责就是保存发生于领域模型中的领域事件，并对事件数据进行归档。
     * 需要解决的事件同步、快照、性能、消息派发等问题。
     */
    public interface IEventStore : IDisposable
    {
        /**
         * Note：
         *  为什么IEventStore接口的SaveEventAsync方法签名中，没有CancellationToken参数？
         *  严格来说，支持async/await异步编程模型的方法定义上，是需要带上CancellationToken参数的，
         *  以便调用方请求取消操作的时候，方法内部可以根据情况对操作进行取消。然而有些情况下取消操作并不是那么合理，
         *  或者方法内部所使用的API并没有提供更深层的取消支持，因此也就没有必要在方法定义上增加CancellationToken参数。
         *  在此处，为了保证接口的简单，没有引入CancellationToken的参数。
         */

        Task SaveEventAsync<TEvent>(TEvent @event)
            where TEvent : IEvent;
    }
}
