using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sample.Common.Events;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Sample.EventStores.Dapper
{
    public class DapperEventStore : IEventStore
    {
        private readonly string connectionString;
        private readonly ILogger logger;

        /**
         * Note: 
         *  此处使用了JsonConvert.SerializeObject方法来序列化事件对象，
         *  也就意味着DapperEventStore程序集需要依赖Newtonsoft.Json程序集。
         *  虽然在我们此处的案例中不会有什么影响，但这样做会造成DapperEventStore对Newtonsoft.Json的强依赖，
         *  这样的依赖关系不仅让DapperEventStore变得不可测试，而且Newtonsoft.Json将来未知的变化，
         *  也会影响到DapperEventStore，带来一些不确定性和维护性问题。更好的做法是，
         *  引入一个IMessageSerializer接口，在另一个新的程序集中使用Newtonsoft.Json来实现这个接口，
         *  同时仅让DapperEventStore依赖IMessageSerializer，并在应用程序启动时，
         *  将Newtonsoft.Json的实现注册到IoC容器中。此时，IMessageSerializer可以被Mock，DapperEventStore就变得可测试了；
         *  另一方面，由于只有那个新的程序集会依赖Newtonsoft.Json，因此，Newtonsoft.Json的变化也仅仅会影响那个新的程序集，
         *  不会对框架主体的其它部分造成任何影响。
         */

        public DapperEventStore(string connectionString, ILogger<DapperEventStore> logger)
        {
            this.connectionString = connectionString;
            this.logger = logger;
            logger.LogInformation($"DapperEventStore 构造函数调用完成。Hash Code：{this.GetHashCode()}.");
        }

        public async Task SaveEventAsync<TEvent>(TEvent @event) where TEvent : IEvent
        {
            logger.LogInformation($"DapperEventStore 正在更新数据库。Hash Code: {this.GetHashCode()}.");

            const string sql = @"INSERT INTO [dbo].[Events] 
                                ([EventId], [EventPayload], [EventTimestamp]) 
                                VALUES 
                                (@eventId, @eventPayload, @eventTimestamp)";

            using (var connection = new SqlConnection(this.connectionString))
            {
                await connection.ExecuteAsync(sql, new
                {
                    eventId = @event.Id,
                    eventPayload = JsonConvert.SerializeObject(@event),
                    eventTimestamp = @event.Timestamp
                });
            }
        }

        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.logger.LogInformation($"DapperEventStore 已经被 Dispose。Hash Code:{this.GetHashCode()}.");
                }

                disposedValue = true;
            }
        }
        public void Dispose() => Dispose(true);
    }
}
