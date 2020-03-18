using EventStore.ClientAPI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectionFramework;
using System;
using System.Collections.Generic;
using System.Net;
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
                var connectionString = "ConnectTo=tcp://admin:changeit@34.254.92.105:1113";
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
            }
        }
    }
}
