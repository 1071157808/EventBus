using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Common.Events
{
    /**
     * 事件总线的重构 BaseEventBus
     * 
     * 通过BaseEventBus的构造函数传入IEventHandlerExecutionContext实例，也就限定了所有子类的实现中，
     * 必须在构造函数中传入IEventHandlerExecutionContext实例，这对于框架的设计非常有利：
     * 在实现新的事件总线时，框架的使用者无需查看API文档，即可知道事件总线与IEventHandlerExecutionContext之间的关系，
     * 这符合SOLID原则中的Open/Closed Principle
     */
    public abstract class BaseEventBus : IEventBus
    {
        protected readonly IEventHandlerExecutionContext eventHandlerExecutionContext;

        protected BaseEventBus(IEventHandlerExecutionContext eventHandlerExecutionContext)
        {
            this.eventHandlerExecutionContext = eventHandlerExecutionContext;
        }

        public abstract Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent;

        public abstract void Subscribe<TEvent, TEventHandler>()
            where TEvent : IEvent
            where TEventHandler : IEventHandler<TEvent>;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~EventBus() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // 不要更改此代码。 将清理代码放在Dispose（布局配置）中。
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
