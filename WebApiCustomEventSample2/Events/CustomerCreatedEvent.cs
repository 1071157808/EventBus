using Sample.Common.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiCustomEventSample2.Events
{
    public class CustomerCreatedEvent : IEvent
    {
        public CustomerCreatedEvent(string customerName)
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
            CustomerName = customerName;
        }

        public Guid Id { get; }

        public DateTime Timestamp { get; }

        public string CustomerName { get; }
    }
}
