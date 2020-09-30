using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RRTest.GrpcService;
using SlimMessageBus;
using SlimMessageBus.Host.AspNetCore;
using SlimMessageBus.Host.Config;
using SlimMessageBus.Host.Kafka;
using SlimMessageBus.Host.Kafka.Configs;
using SlimMessageBus.Host.Serialization.Json;
using GetAccountsRequest = RRTest.Contracts.GetAccountsRequest;

namespace RRTest.Client.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddGrpcClient<ClientAccounts.ClientAccountsClient>(o =>
            {
                o.Address = new Uri("https://localhost:5001");
            });

            services.AddHttpClient<ClientAccountsRestClient>(c =>
                {
                    c.BaseAddress = new Uri("http://localhost:51190");
                    c.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                    c.DefaultRequestHeaders.Add("User-Agent", "RRTest.Client.Api");
                }

            );

            // register MessageBus  
            ConfigureMessageBus(services);

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen();
        }

        private void ConfigureMessageBus(IServiceCollection services)
        {
            services.AddHttpContextAccessor(); // This is required for the SlimMessageBus.Host.AspNetCore plugin

            services.AddSingleton<IMessageBus>(BuildMessageBus);

            services.AddSingleton<IRequestResponseBus>(svp => svp.GetService<IMessageBus>());

            // register any consumers (IConsumer<>) or handlers (IHandler<>) - if any
        }

        private IMessageBus BuildMessageBus(IServiceProvider serviceProvider)
        {
            // unique id across instances of this application (e.g. 1, 2, 3)
            var instanceId = Configuration["InstanceId"];
            var kafkaBrokers = Configuration["Kafka:Brokers"];

            var instanceGroup = $"webapi-{instanceId}";
            var instanceReplyTo = $"webapi-{instanceId}-response";

            var messageBusBuilder = MessageBusBuilder.Create()
                .Produce<GetAccountsRequest>(x =>
                {
                    // Default response timeout for this request type
                    //x.DefaultTimeout(TimeSpan.FromSeconds(10));
                    x.DefaultTopic("accounts-service");
                })
                .ExpectRequestResponses(x =>
                {
                    x.ReplyToTopic(instanceReplyTo);
                    x.Group(instanceGroup);
                    // Default global response timeout
                    x.DefaultTimeout(TimeSpan.FromSeconds(60));
                })
                .WithDependencyResolver(new AspNetCoreMessageBusDependencyResolver(serviceProvider))
                .WithSerializer(new JsonMessageSerializer())
                .WithProviderKafka(new KafkaMessageBusSettings(kafkaBrokers)
                {
                    ProducerConfigFactory = () => new Dictionary<string, object>
                    {
                        {"socket.blocking.max.ms",1},
                        {"queue.buffering.max.ms",1},
                        {"socket.nagle.disable", true},
                        {"message.max.bytes",  52428800}
                    },
                    ConsumerConfigFactory = (group) => new Dictionary<string, object>
                    {
                        {"socket.blocking.max.ms", 1},
                        {"fetch.error.backoff.ms", 1},
                        {"statistics.interval.ms", 500000},
                        {"socket.nagle.disable", true},
                        {"max.partition.fetch.bytes",  10048576},
                        {"fetch.max.bytes", 52428800}
                    }
                });

            var messageBus = messageBusBuilder.Build();
            return messageBus;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseRouting();

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            var messageBus = app.ApplicationServices.GetRequiredService<IMessageBus>();
        }
    }
}
