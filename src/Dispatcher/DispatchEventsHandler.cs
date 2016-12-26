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