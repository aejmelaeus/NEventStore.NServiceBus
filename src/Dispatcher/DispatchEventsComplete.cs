using NServiceBus;

namespace NEventStore.NServiceBus
{
    public class DispatchEventsComplete : ICommand
    {
        public string CheckpointToken { get; set; }
        public string BucketId { get; set; }
    }
}