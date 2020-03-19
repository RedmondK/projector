using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;
using ProjectionFramework;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Projector
{
    class Program
    {
        static ManualResetEvent _quitEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            var logger = loggerFactory.CreateLogger<Program>();

            Console.CancelKeyPress += (sender, eArgs) =>
            {
                _quitEvent.Set();
                eArgs.Cancel = true;
            };

            try
            {
                var connectionString = "ConnectTo=tcp://admin:fencloudnative2020@54.229.179.193:1113";
                var settingsBuilder = ConnectionSettings.Create();

                var eventStoreConnection = EventStoreConnection.Create(connectionString, settingsBuilder, "Projector");
                eventStoreConnection.ConnectAsync().Wait();

                var mongoRepository = new MongoDAL.MongoDBRepository("mongodb+srv://projector:projector@cluster0-gr1bz.mongodb.net/test?retryWrites=true&w=majority");

                var projections = new List<IProjection>
                {
                    new CaseProjection(mongoRepository)
                    ,new EntityProjection(mongoRepository)
                };

                var p = new ProjectionFramework.Projector(eventStoreConnection, projections, mongoRepository);

                logger.LogInformation("Starting Projector");

                p.Start();

                logger.LogInformation("Projector Started");

                _quitEvent.WaitOne();
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                logger.LogError(e.StackTrace);
                logger.LogError(e.InnerException.Message);
                logger.LogError(e.InnerException.StackTrace);
            }
        }
    }
}
