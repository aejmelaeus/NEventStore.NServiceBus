using System;
using System.IO;
using System.Threading;
using NEventStore;
using Tests.Messages;
using NUnit.Framework;
using NEventStore.Persistence.Sql.SqlDialects;

namespace Tests.Acceptance
{
    [TestFixture]
    public class DispatcherAcceptanceTest
    {
        private readonly string _connectionString = Environment.GetEnvironmentVariables().Contains("APPVEYOR")
            ? @"Server=(local)\SQL2014;Initial Catalog=Dispatcher;User ID=sa;Password=Password12!"
            : @"Data Source=SE-UTV28172; Initial Catalog=Dispatcher; Integrated Security=True";

        [Test]
        public void WhenCommittingEvents_EventsPublishedThroughDispatcher()
        {
            /*
            ** This test expects that the Dispatcher Endpoint is running, in this case Test.Acceptance.Endpoint
            ** - Commit an event to the event store
            ** - Wait
            ** - Check in a file (hmm... but it works) that the event was processed and thereby published
            **
            ** So here we GO:
            */

            string theId = Guid.NewGuid().ToString();

            var theEvent = new EventHappened
            {
                TheStuff = theId
            };

            using (var eventSource = GetEventSource())
            using (var stream = eventSource.CreateStream(theId))
            {
                stream.Add(new EventMessage { Body = theEvent });
                stream.CommitChanges(Guid.NewGuid());
            }

            // Act
            Thread.Sleep(10000);
            var result = File.ReadAllText(@"C:\temp\DispatcherAcceptanceTestResults.txt");

            // Assert
            Assert.That(result.Contains(theId));
        }

        private IStoreEvents GetEventSource()
        {
            return Wireup
                .Init()
                .UsingSqlPersistence("SequencedAggregate", "System.Data.SqlClient", _connectionString)
                    .WithDialect(new MsSqlDialect())
                    .EnlistInAmbientTransaction()
                .InitializeStorageEngine()
                    .UsingJsonSerialization()
                    .Compress()
                .Build();
        }
    }
}
