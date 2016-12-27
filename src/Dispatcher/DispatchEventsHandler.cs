using System.Threading.Tasks;
using NEventStore.Persistence;
using NServiceBus;

namespace NEventStore.NServiceBus
{
    public class DispatchEventsHandler : IHandleMessages<DispatchEvents>
    {
        private readonly IPersistStreams _persistStreams;

        public DispatchEventsHandler(IStoreEvents storeEvents)
        {
            _persistStreams = storeEvents.Advanced;
        }

        public Task Handle(DispatchEvents message, IMessageHandlerContext context)
        {
            var commits = _persistStreams.GetFrom(message.BucketId, message.CheckpointToken);

            var checkpoint = new Checkpoint(commits, message.CheckpointToken, message.MessageCatalogAssemblyName);

            foreach (var eventToPublish in checkpoint.EventsToPublish)
            {
                context.Publish(eventToPublish);
            }

            return context.Send(new DispatchEventsComplete
            {
                BucketId = message.BucketId,
                CheckpointToken = checkpoint.NewCheckpointToken
            });
        }
    }
}