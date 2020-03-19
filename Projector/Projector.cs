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
        private IConfiguration Config { get; }
        private ILogger<Projector> Logger { get; }
        private CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
        private TaskCompletionSource<bool> TaskCompletionSource { get; } = new TaskCompletionSource<bool>();

        public Projector(IConfiguration config, ILoggerFactory loggerFactory)
        {
            Config = config;
            Logger = loggerFactory.CreateLogger<Projector>();
        }

        public async Task Project()
        {
            try
            {
                var connectionString = Config.GetValue<string>("eventStoreConnectionString");
                var settingsBuilder = ConnectionSettings.Create();

                var eventStoreConnection = EventStoreConnection.Create(connectionString, settingsBuilder, Config.GetValue<string>("eventStoreConnectionName"));
                eventStoreConnection.ConnectAsync().Wait();

                var mongoRepository = new MongoDAL.MongoDBRepository(Config.GetValue<string>("stateDbConnectionString"));

                var projections = new List<IProjection>
                {
                    new CaseProjection(mongoRepository)
                    ,new EntityProjection(mongoRepository)
                };

                var p = new ProjectionFramework.Projector(eventStoreConnection, projections, mongoRepository);

                Logger.LogInformation("Starting Projector");

                await Task.Run(() => p.Start());

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
            Task.Run(() => Project());
            return Task.CompletedTask;

        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Projector Stopping");
            TaskCompletionSource.SetResult(true);
            return TaskCompletionSource.Task;
        }
    }
}
