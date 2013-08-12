using System;
using Bottles.Services.Messaging.Tracking;
using FubuTransportation.Logging;
using FubuTransportation.Runtime;
using FubuTransportation.TestSupport;
using NUnit.Framework;
using System.Linq;
using FubuTestingSupport;

namespace FubuTransportation.Testing.TestSupport
{
    [TestFixture]
    public class MessageWatcherTester
    {
        [Test]
        public void handle_chain_started()
        {
            MessageHistory.ClearAll();

            var @event = new ChainExecutionStarted
            {
                ChainId = Guid.NewGuid(), Envelope = new Envelope()
            };
            new MessageWatcher().Handle(@event);

            var sent = MessageHistory.Outstanding().Single();
            sent.Id.ShouldEqual(@event.Envelope.CorrelationId);
            sent.Description.ShouldEqual(@event.ToString());
            sent.Type.ShouldEqual(MessageWatcher.MessageTrackType);
        }

        [Test]
        public void handle_chain_finished()
        {
            MessageHistory.ClearAll();

            var @event = new ChainExecutionStarted
            {
                ChainId = Guid.NewGuid(),
                Envelope = new Envelope()
            };
            var messageWatcher = new MessageWatcher();
            messageWatcher.Handle(@event);

            var finished = new ChainExecutionFinished
            {
                ChainId = @event.ChainId,
                Envelope = @event.Envelope
            };

            messageWatcher.Handle(finished);

            var received = MessageHistory.Received().Single();
            received.Id.ShouldEqual(@event.Envelope.CorrelationId);
            received.Description.ShouldEqual(finished.ToString());
            received.Type.ShouldEqual(MessageWatcher.MessageTrackType);

            MessageHistory.Outstanding().Any().ShouldBeFalse();
        }
    }
}