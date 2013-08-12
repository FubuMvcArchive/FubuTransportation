using System;
using FubuTestingSupport;
using FubuTransportation.Runtime;
using NUnit.Framework;

namespace FubuTransportation.Testing.Runtime
{
    [TestFixture]
    public class EnvelopeTester
    {
        [Test]
        public void default_values_for_original_and_parent_id_are_null()
        {
            var parent = new Envelope
            {
                CorrelationId = Guid.NewGuid().ToString()
            };

            parent.OriginalId.ShouldBeNull();
            parent.ParentId.ShouldBeNull();
        }

        [Test]
        public void original_message_creating_child_envelope()
        {
            var parent = new Envelope
            {
                CorrelationId = Guid.NewGuid().ToString()
            };

            var childMessage = new Message1();

            var child = parent.ForResponse(childMessage);

            child.Message.ShouldBeTheSameAs(childMessage);

            child.OriginalId.ShouldEqual(parent.CorrelationId);
            child.ParentId.ShouldEqual(parent.CorrelationId);
        }

        [Test]
        public void parent_that_is_not_original_creating_child_envelope()
        {
            var parent = new Envelope
            {
                CorrelationId = Guid.NewGuid().ToString(),
                OriginalId = Guid.NewGuid().ToString()
            };

            var childMessage = new Message1();

            var child = parent.ForResponse(childMessage);

            child.Message.ShouldBeTheSameAs(childMessage);

            child.OriginalId.ShouldEqual(parent.OriginalId);
            child.ParentId.ShouldEqual(parent.CorrelationId);
        }

        [Test]
        public void if_reply_requested_header_exists_in_parent()
        {
            var parent = new Envelope
            {
                CorrelationId = Guid.NewGuid().ToString(),
                OriginalId = Guid.NewGuid().ToString(),
                Source = "foo://bar".ToUri()
            };

            parent.Headers[Envelope.ReplyRequested] = true.ToString();

            var childMessage = new Message1();

            var child = parent.ForResponse(childMessage);

            child.Headers[Envelope.Response].ShouldEqual(true.ToString());
            child.Destination.ShouldEqual(parent.Source);
        }

        [Test]
        public void do_not_set_destination_or_response_if_requested_header_does_not_exist_in_parent()
        {
            var parent = new Envelope
            {
                CorrelationId = Guid.NewGuid().ToString(),
                OriginalId = Guid.NewGuid().ToString(),
                Source = "foo://bar".ToUri()
            };

            parent.Headers.Has(Envelope.ReplyRequested).ShouldBeFalse();

            var childMessage = new Message1();

            var child = parent.ForResponse(childMessage);

            child.Headers.Has(Envelope.Response).ShouldBeFalse();
            child.Destination.ShouldBeNull();
        }
    }
}