using NServiceBus;

namespace NEventStore.NServiceBus
{
    public class DispatchEvents : ICommand
    {
        public string CheckpointToken { get; set; }
        public string BucketId { get; set; }
        public string MessageCatalogAssemblyName { get; set; }
    }
}