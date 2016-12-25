using NServiceBus;

namespace Dispatcher
{
    public class DispatchEventsComplete : ICommand
    {
        public string CheckpointToken { get; set; }
        public string BucketId { get; set; }
    }
}