using NServiceBus;

namespace Dispatcher
{
    public class DispatchEvents : ICommand
    {
        public string CheckpointToken { get; set; }
        public string BucketId { get; set; }
    }
}