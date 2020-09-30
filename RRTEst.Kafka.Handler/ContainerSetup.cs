using System;
using Autofac;
using Microsoft.Extensions.Configuration;
using RRTest.Contracts;
using RRTest.Service;
using RRTEst.Kafka.Handler.Handlers;
using SlimMessageBus;
using SlimMessageBus.Host.Autofac;
using SlimMessageBus.Host.Config;
using SlimMessageBus.Host.Kafka;
using SlimMessageBus.Host.Kafka.Configs;
using SlimMessageBus.Host.Serialization.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RRTEst.Kafka.Handler
{
    public static class ContainerSetup
    {
        public static IContainer Create(IConfigurationRoot configuration)
        {
            var builder = new ContainerBuilder();

            var path = AppDomain.CurrentDomain.BaseDirectory;
            foreach (var assembly in Directory.GetFiles(path, "RRTest.Contracts.*dll").Select(Assembly.LoadFrom))
            {
                builder.RegisterAssemblyTypes(assembly).AsSelf();
            }

            Configure(builder, configuration);

            var container = builder.Build();
            AutofacMessageBusDependencyResolver.Container = container;
            return container;
        }

        private static void Configure(ContainerBuilder builder, IConfigurationRoot configuration)
        {
            builder.Register(x => new AccountsService()).As<IAccountsService>().SingleInstance();

            // SlimMessageBus
            builder.Register(x => BuildMessageBus(configuration))
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<GetAccountsHandler>().AsSelf();
        }

        private static IMessageBus BuildMessageBus(IConfigurationRoot configuration)
        {
            var instanceId = configuration["InstanceId"];
            var kafkaBrokers = configuration["Kafka:Brokers"];

            var instanceGroup = $"worker-{instanceId}";
            var sharedGroup = "workers";

            var messageBusBuilder = MessageBusBuilder
                .Create()
                .Handle<GetAccountsRequest, GetAccountsResponse>(s =>
                {
                    s.Topic("accounts-service", t =>
                    {
                        t.WithHandler<GetAccountsHandler>()
                            .Group(sharedGroup)
                            .Instances(10);
                    });
                })
                .WithDependencyResolver(new AutofacMessageBusDependencyResolver())
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
                    ConsumerConfigFactory = group => new Dictionary<string, object>
                    {
                        {"socket.blocking.max.ms", 1},
                        {"fetch.error.backoff.ms", 1},
                        {"socket.nagle.disable", true},
                        {"message.max.bytes",  52428800},
                        {"max.partition.fetch.bytes",  10048576},
                        {"fetch.max.bytes", 52428800},
                        {KafkaConfigKeys.ConsumerKeys.AutoCommitEnableMs, 5000},
                        {KafkaConfigKeys.ConsumerKeys.StatisticsIntervalMs, 500000},
                        {
                            "default.topic.config", new Dictionary<string, object>
                            {
                                {KafkaConfigKeys.ConsumerKeys.AutoOffsetReset, KafkaConfigValues.AutoOffsetReset.Latest}
                            }
                        }
                    }
                });

            var messageBus = messageBusBuilder.Build();
            return messageBus;
        }
    }
}
