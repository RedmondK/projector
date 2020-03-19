using EventStore.ClientAPI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectionFramework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Projector
{
    public class Projector : IHostedService
    {
        private IConfigurationRoot Config { get; }
        private ILogger<Projector> Logger { get; }
        private CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
        private TaskCompletionSource<bool> TaskCompletionSource { get; } = new TaskCompletionSource<bool>();

        public Projector(IConfigurationRoot config, ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger<Projector>();
            Config = config;
        }

        public void Project()
        {
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

                Logger.LogInformation("Starting Projector");

                p.Start();

                Logger.LogInformation("Projector Started");
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                Logger.LogError(e.StackTrace);
                Logger.LogError(e.InnerException.Message);
                Logger.LogError(e.InnerException.StackTrace);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
