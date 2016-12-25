using Dispatcher;
using NEventStore;
using NSubstitute;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using Tests.Messages;

namespace Tests.Unit
{
    [TestFixture]
    public class CheckpointTests
    {
        [Test]
        public void TODOS()
        {
            // Check that the assembly is set
            // Check that the milliseconds are set
        }

        [Test]
        public void NewChecpointToken_WhenCheckpointContainsNoCommits_NewCheckpointTokenSameAsInitial()
        {
            // Arrange
            const string checkpointToken = "SomeCheckpointToken";

            var checkpoint = new Checkpoint(new List<ICommit>(), checkpointToken, "SomeAssemblyName");

            // Act & Assert
            Assert.That(checkpoint.NewCheckpointToken, Is.EqualTo(checkpointToken));
        }

        [Test]
        public void EventsToPublish_WhenCheckpointContainsNoCommits_EventsToPublishIsEmpty()
        {
            // Arrange
            var checkpoint = new Checkpoint(new List<ICommit>(), "SomeCheckpointToken", "SomeAssemblyName");

            // Act & Assert
            Assert.That(checkpoint.EventsToPublish, Is.Empty);
        }

        [Test]
        public void NewCheckpointToken_WhenCheckpointsContainsCommits_NewCheckpointTokenTakenFromLastCommit()
        {
            // Arrange
            var firstCommit = Substitute.For<ICommit>();
            firstCommit.CheckpointToken.Returns("FirstCommit");

            var secondCommit = Substitute.For<ICommit>();
            secondCommit.CheckpointToken.Returns("SecondCommit");

            var checkpoint = new Checkpoint(new [] { firstCommit, secondCommit }, "InitialCheckpointToken", "SomeAssemblyName");

            // Act & Assert
            Assert.That(checkpoint.NewCheckpointToken, Is.EqualTo("SecondCommit"));
        }

        [Test]
        public void EventsToPublish_WhenCommitContainsAEventThatIsInTheAssembly_TheEventIsNotInEventsToPublish()
        {
            // Arrange
            var theEvent = new EventHappened();
            var eventMessage = new EventMessage { Body = theEvent };
            var commit = Substitute.For<ICommit>();
            commit.Events.Returns(new[] { eventMessage });
            
            var checkpoint = new Checkpoint(new [] { commit }, "SomeCheckpointToken", "tests.unit.Messages");
            
            // Act & Assert
            Assert.That(checkpoint.EventsToPublish.First(), Is.SameAs(theEvent));
        }

        [Test]
        public void EventsToPublish_WhenCommitContainsAEventThatIsNotInTheAssembly_TheEventIsNotInEventsToPublish()
        {
            // Arrange
            var internalEvent = new InternalEvent();
            var eventMessage = new EventMessage { Body = internalEvent };
            var commit = Substitute.For<ICommit>();
            commit.Events.Returns(new[] { eventMessage });

            var checkpoint = new Checkpoint(new[] { commit }, "SomeCheckpointToken", "tests.unit.Messages");

            // Act & Assert
            Assert.That(checkpoint.EventsToPublish, Is.Empty);
        }
    }
}
 