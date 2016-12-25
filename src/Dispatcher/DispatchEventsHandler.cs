using NServiceBus;
using System.Configuration;
using System.Threading.Tasks;
using NEventStore.Persistence;

namespace Dispatcher
{
    public class DispatchEventsHandler : IHandleMessages<DispatchEvents>
    {
        private readonly IPersistStreams _persistStreams;

        public DispatchEventsHandler(IPersistStreams persistStreams)
        {
            _persistStreams = persistStreams;
        }

        public Task Handle(DispatchEvents message, IMessageHandlerContext context)
        {
            var bucketId = message.BucketId;
            var checkpointToken = message.CheckpointToken;
            var messageCatalogAssemblyName = ConfigurationManager.AppSettings["MessageCatalogAssemblyName"];

            var commits = _persistStreams.GetFrom(message.BucketId, checkpointToken);

            var checkpoint = new Checkpoint(commits, checkpointToken, messageCatalogAssemblyName);

            foreach (var eventToPublish in checkpoint.EventsToPublish)
            {
                context.Publish(eventToPublish);
            }

            return context.Send(new DispatchEventsComplete
            {
                BucketId = bucketId,
                CheckpointToken = checkpoint.NewCheckpointToken
            });
        }
    }
}