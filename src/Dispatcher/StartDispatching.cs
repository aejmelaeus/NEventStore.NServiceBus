using NServiceBus;

namespace NEventStore.NServiceBus
{
    public class StartDispatching : ICommand
    {
        public string BucketId { get; set; }
        public int TimeoutInMilliseconds { get; set; }
        public string MessageCatalogAssemblyName { get; set; }
    }
}
