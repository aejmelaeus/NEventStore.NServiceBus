using NEventStore.NServiceBus;
using NUnit.Framework;
using NServiceBus.Testing;

namespace Tests.Unit
{
    public class DispatcherSagaTests
    {
        [Test]
        public void HandleStartDispatching_WhenSagaNotStarted_DispatchEventSentWithCorrectBuckedId()
        {
            const string bucketId = "default";

            Test.Saga<DispatcherSaga>()
                .ExpectSend<DispatchEvents>(c => c.BucketId == bucketId && c.CheckpointToken == string.Empty)
                .When((saga, context) => saga.Handle(new StartDispatching { BucketId = bucketId }, context));
        }

        [Test]
        public void HandleStartDispatching_WhenSagaNotStarted_HasStartedAndBuckedIdSetAndTimeoutSet()
        {
            const string bucketId = "default";
            const int milliseconds = 1000;
            const string messageCatalogAssemblyName = "SomeAssembly";

            Test.Saga<DispatcherSaga>()
                .ExpectSagaData<DispatcherSagaData>(dsd => 
                    dsd.HasStarted.Equals(true) && 
                    dsd.BucketId.Equals(bucketId) && 
                    dsd.TimeOutInMilliseconds.Equals(milliseconds) &&
                    dsd.MessageCatalogAssemblyName.Equals(messageCatalogAssemblyName))
                .When((saga, context) => saga.Handle(new StartDispatching
                {
                    BucketId = bucketId,
                    TimeoutInMilliseconds = milliseconds,
                    MessageCatalogAssemblyName = messageCatalogAssemblyName
                }, context));
        }

        [Test]
        public void HandleStartDispatching_WhenSagaStarted_NoDispatchEventsCommandSent()
        {
            var sagaData = new DispatcherSagaData
            {
                HasStarted = true
            };

            Test.Saga<DispatcherSaga>(sagaData)
                .ExpectNotSend<DispatchEvents>()
                .When((saga, context) => saga.Handle(new StartDispatching(), context));
        }

        [Test]
        public void HandleDispatchEventsComplete_WhenHandled_BuckedIdAndTimeoutSet()
        {
            const string checkpointToken = "SomeCheckpointToken";
            const int milliseconds = 3000;

            var sagaData = new DispatcherSagaData
            {
                TimeOutInMilliseconds = milliseconds
            };

            Test.Saga<DispatcherSaga>(sagaData)
                .ExpectTimeoutToBeSetIn<DispatcherSagaTimeout>((state, span) => span.TotalMilliseconds.Equals(milliseconds))
                .ExpectSagaData<DispatcherSagaData>(dsd => dsd.CheckpointToken.Equals(checkpointToken))
                .When((saga, context) => saga.Handle(new DispatchEventsComplete { CheckpointToken = checkpointToken }, context));
        }

        [Test]
        public void Timeout_WhenSagaTimesOut_DispatchEventsCommandSentWithCorrectBuckedIdAndCheckpointToken()
        {
            // Arrange
            const string checkpointToken = "SomeCheckpointToken";
            const string bucketId = "SomeBucketId";
            const string messageCatalogAssemblyName = "SomeAssemblyName";

            var sagaData = new DispatcherSagaData
            {
                CheckpointToken = checkpointToken,
                BucketId = bucketId,
                MessageCatalogAssemblyName = messageCatalogAssemblyName
            };

            // Act & Assert
            Test.Saga<DispatcherSaga>(sagaData)
                .ExpectSend<DispatchEvents>(c => 
                    c.BucketId.Equals(bucketId) && 
                    c.CheckpointToken.Equals(checkpointToken) &&
                    c.MessageCatalogAssemblyName.Equals(messageCatalogAssemblyName)
                )
                .WhenHandlingTimeout<DispatcherSagaTimeout>();
        }
    }
}
