using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using ProjectionFramework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Projector
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = "ConnectTo=tcp://admin:changeit@34.254.92.105:1113";
            var settingsBuilder = ConnectionSettings.Create();

            var eventStoreConnection = EventStoreConnection.Create(connectionString, settingsBuilder, "LOCAL");
            eventStoreConnection.ConnectAsync().Wait();

            var mongoRepository = new MongoDAL.MongoDBRepository("mongodb+srv://projector:projector@cluster0-gr1bz.mongodb.net/test?retryWrites=true&w=majority");

            var projections = new List<IProjection>
            {
                new CaseProjection(mongoRepository)
                ,new EntityProjection(mongoRepository)
            };

            var p = new ProjectionFramework.Projector(eventStoreConnection, projections, mongoRepository);

            p.Start();

            Console.WriteLine("Projector Running");
            Console.ReadLine();
        }
    }
}
