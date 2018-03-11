using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sample.Common.Events;
using WebApiCustomEventSample2.Events;

namespace WebApiCustomEventSample2.Controllers
{
    [Produces("application/json")]
    [Route("api/Customers")]
    public class CustomersController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly string connectionString;
        private readonly IEventBus eventBus;
        private readonly ILogger logger;

        public CustomersController(IConfiguration configuration,
            IEventBus eventBus, ILogger<CustomersController> logger)
        {
            this.configuration = configuration;
            connectionString = configuration["sql:connectionString"];
            this.eventBus = eventBus;
            this.logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            //guid 99ff3968-7ddd-4caf-9e33-c9ec7582c87e
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] dynamic model)
        {
            //PostMan raw {Name:'yxf007'} //区分大小写

            if (model == null)
                return BadRequest();

            var name = (string)model.Name;
            if (string.IsNullOrEmpty(name))
                return BadRequest();

            logger.LogInformation($"假设客户信息创建成功。");

            await eventBus.PublishAsync(new CustomerCreatedEvent(name));

            return Ok();
        }
    }
}