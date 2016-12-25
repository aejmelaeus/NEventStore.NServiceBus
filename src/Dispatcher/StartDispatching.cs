using NServiceBus;

namespace Dispatcher
{
    public class StartDispatching : ICommand
    {
        public string BucketId { get; set; }
        public int TimeoutInMilliseconds { get; set; }
    }
}
