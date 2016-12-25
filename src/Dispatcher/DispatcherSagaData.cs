using NServiceBus;

namespace Dispatcher
{
    public class DispatcherSagaData : ContainSagaData
    {
        public virtual string CheckpointToken { get; set; }
        public virtual bool HasStarted { get; set; }
        public virtual string BucketId { get; set; }
        public virtual int TimeOutInMilliseconds { get; set; }
    }
}