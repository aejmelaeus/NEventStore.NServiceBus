using NServiceBus;

namespace Tests.Messages
{
    public class EventHappened : EventBase, IEvent
    {
        public string TheStuff { get; set; }
    }
}
