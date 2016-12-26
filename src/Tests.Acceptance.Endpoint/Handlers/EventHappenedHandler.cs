using System;
using System.IO;
using NServiceBus;
using Tests.Messages;
using System.Threading.Tasks;

namespace Tests.Acceptance.Endpoint.Handlers
{
    public class EventHappenedHandler : IHandleMessages<EventHappened>
    {
        private readonly string _filePath = Environment.GetEnvironmentVariables().Contains("APPVEYOR")
            ? @"C:\projects\neventstore-nservicebus\DispatcherAcceptanceTestResults.txt"
            : @"C:\temp\DispatcherAcceptanceTestResults.txt";

        public Task Handle(EventHappened message, IMessageHandlerContext context)
        {
            File.AppendAllText(_filePath, message.TheStuff + Environment.NewLine);
            return Task.FromResult(0);
        }
    }
}
