using System;
using System.Threading.Tasks;
using NServiceBus;

namespace NEventStore.NServiceBus
{
    public class DispatcherSaga : Saga<DispatcherSagaData>,
        IAmStartedByMessages<StartDispatching>,
        IHandleMessages<DispatchEventsComplete>,
        IHandleTimeouts<DispatcherSagaTimeout>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<DispatcherSagaData> mapr)
        {
            mapr.ConfigureMapping<StartDispatching>(c => c.BucketId)
                .ToSaga(sagaData => sagaData.BucketId);

            mapr.ConfigureMapping<DispatchEventsComplete>(c => c.BucketId)
                .ToSaga(sagaData => sagaData.BucketId);
        }

        public Task Handle(StartDispatching message, IMessageHandlerContext context)
        {
            if (Data.HasStarted)
            {
                return Task.FromResult(0);
            }

            Data.HasStarted = true;
            Data.BucketId = message.BucketId;
            Data.TimeOutInMilliseconds = message.TimeoutInMilliseconds;
            Data.MessageCatalogAssemblyName = message.MessageCatalogAssemblyName;

            return context.Send(new DispatchEvents
            {
                BucketId = message.BucketId,
                CheckpointToken = string.Empty,
                MessageCatalogAssemblyName = Data.MessageCatalogAssemblyName
            });
        }

        public Task Handle(DispatchEventsComplete message, IMessageHandlerContext context)
        {
            Data.CheckpointToken = message.CheckpointToken;
            return RequestTimeout<DispatcherSagaTimeout>(context, TimeSpan.FromMilliseconds(Data.TimeOutInMilliseconds));
        }

        public Task Timeout(DispatcherSagaTimeout state, IMessageHandlerContext context)
        {
            return context.Send(new DispatchEvents
            {
                BucketId = Data.BucketId,
                CheckpointToken = Data.CheckpointToken,
                MessageCatalogAssemblyName = Data.MessageCatalogAssemblyName
            });
        }
    }
}