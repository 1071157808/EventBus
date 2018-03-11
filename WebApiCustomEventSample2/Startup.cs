using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Sample.Common.Events;
using Sample.EventBus.RabbitMQ;
using Sample.EventBus.Simple;
using Sample.EventStores.Dapper;
using Sample.Integration.NetCore;
using WebApiCustomEventSample2.EventHandlers;
using WebApiCustomEventSample2.Events;

namespace WebApiCustomEventSample2
{
    public class Startup
    {
        private const string RMQ_EXCHANGE = "Yxf.Exchange";
        private const string RMQ_QUEUE = "Yxf.Queue";

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;

            logger = loggerFactory.CreateLogger<Startup>();
        }

        private readonly ILogger logger;
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            logger.LogInformation("正在对服务进行配置...");

            services.AddMvc();

            services.AddTransient<IEventStore>(serviceProvider =>
                new DapperEventStore(Configuration["sql:connectionString"],
                serviceProvider.GetRequiredService<ILogger<DapperEventStore>>()));
            //services.AddTransient<IEventHandler, CustomerCreatedEventHandler>();

            services.AddSingleton<IEventHandlerExecutionContext>(new EventHandlerExecutionContext(services, sc => sc.BuildServiceProvider()));
            //services.AddSingleton<IEventBus, PassThroughEventBus>();
            var connectionFactory = new ConnectionFactory { HostName = "localhost" };
            services.AddSingleton<IEventBus>(sp => new RabbitMQEventBus(connectionFactory,
                sp.GetRequiredService<ILogger<RabbitMQEventBus>>(),
                sp.GetRequiredService<IEventHandlerExecutionContext>(),
                RMQ_EXCHANGE,
                queueName: RMQ_QUEUE));

            logger.LogInformation("服务配置完成，已注册到IoC容器！");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // subscribe 将事件处理 注册为瞬时的
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            eventBus.Subscribe<CustomerCreatedEvent, CustomerCreatedEventHandler>();
            //eventBus.Subscribe<TestEvent, TestEventHandler>();  //更多注册

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
