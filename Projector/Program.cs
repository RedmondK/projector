using EventStore.ClientAPI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectionFramework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Projector
{
    class Program
    {
        static ManualResetEvent _quitEvent = new ManualResetEvent(false);

        static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder().ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<Projector>();
            });
        }
    }
}
