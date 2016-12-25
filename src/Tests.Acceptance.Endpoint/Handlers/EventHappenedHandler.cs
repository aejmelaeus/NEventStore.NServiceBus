using System;
using System.IO;
using NServiceBus;
using Tests.Messages;
using System.Threading.Tasks;

namespace Tests.Acceptance.Endpoint.Handlers
{
    public class EventHappenedHandler : IHandleMessages<EventHappened>
    {
        public Task Handle(EventHappened message, IMessageHandlerContext context)
        {
            File.AppendAllText(@"C:\temp\DispatcherAcceptanceTestResults.txt", message.TheStuff + Environment.NewLine);
            return Task.FromResult(0);
        }
    }
}
