using System;
using System.Reflection;
using Autofac;
using Microsoft.Extensions.Configuration;
using Serilog;
using SlimMessageBus;


namespace RRTEst.Kafka.Handler
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            logger.Information("Starting worker...");
            using (var container = ContainerSetup.Create(configuration))
            {
                // eager load the singleton, so that is starts consuming messages
                var messageBus = container.Resolve<IMessageBus>();
                logger.Information("Worker ready");

                Console.WriteLine("Press enter to stop the application...");
                Console.ReadLine();

                logger.Information("Stopping worker...");
            }
            logger.Information("Worker stopped");
        }
    }
}
