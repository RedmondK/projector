using EventStore.ClientAPI;
using ProjectionFramework;
using System;
using System.Collections.Generic;
using System.Net;

namespace Projector
{
    class Program
    {
        static void Main(string[] args)
        {
            var eventStoreConnection = EventStoreConnection.Create(
               ConnectionSettings.Default,
               new IPEndPoint(IPAddress.Loopback, 1113));

            eventStoreConnection.ConnectAsync().Wait();

            var projections = new List<IProjection>
            {
                new CaseProjection()
                ,new EntityProjection()
            };

            var p = new ProjectionFramework.Projector(eventStoreConnection, projections, new MongoDAL.MongoDBRepository("mongodb+srv://projector:projector@cluster0-gr1bz.mongodb.net/test?retryWrites=true&w=majority"));

            p.Start();

            Console.WriteLine("Projector Running");
            Console.ReadLine();
        }
    }
}
