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
        public void has_a_correlation_id_by_default()
        {
            new Envelope().CorrelationId.ShouldNotBeNull();

            new Envelope().CorrelationId.ShouldNotEqual(new Envelope().CorrelationId);
            new Envelope().CorrelationId.ShouldNotEqual(new Envelope().CorrelationId);
            new Envelope().CorrelationId.ShouldNotEqual(new Envelope().CorrelationId);
            new Envelope().CorrelationId.ShouldNotEqual(new Envelope().CorrelationId);
            new Envelope().CorrelationId.ShouldNotEqual(new Envelope().CorrelationId);
        }

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

            parent.Headers[Envelope.ReplyRequestedKey] = true.ToString();

            var childMessage = new Message1();

            var child = parent.ForResponse(childMessage);

            child.Headers[Envelope.ResponseIdKey].ShouldEqual(parent.CorrelationId);
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

            parent.Headers.Has(Envelope.ReplyRequestedKey).ShouldBeFalse();

            var childMessage = new Message1();

            var child = parent.ForResponse(childMessage);

            child.Headers.Has(Envelope.ResponseIdKey).ShouldBeFalse();
            child.Destination.ShouldBeNull();
        }

        [Test]
        public void source_property()
        {
            var envelope = new Envelope();

            envelope.Source.ShouldBeNull();

            var uri = "fake://thing".ToUri();
            envelope.Source = uri;

            envelope.Headers[Envelope.SourceKey].ShouldEqual(uri.ToString());
            envelope.Source.ShouldEqual(uri);
        }

        [Test]
        public void content_type()
        {
            var envelope = new Envelope();
            envelope.ContentType.ShouldEqual(null);

            envelope.ContentType = "text/xml";

            envelope.Headers[Envelope.ContentTypeKey].ShouldEqual("text/xml");
            envelope.ContentType.ShouldEqual("text/xml");
        }

        [Test]
        public void original_id()
        {
            var envelope = new Envelope();
            envelope.OriginalId.ShouldBeNull();

            var originalId = Guid.NewGuid().ToString();
            envelope.OriginalId = originalId;

            envelope.Headers[Envelope.OriginalIdKey].ShouldEqual(originalId);
            envelope.OriginalId.ShouldEqual(originalId);
        }

        [Test]
        public void ParentId()
        {
            var envelope = new Envelope();
            envelope.ParentId.ShouldBeNull();

            var parentId = Guid.NewGuid().ToString();
            envelope.ParentId = parentId;

            envelope.Headers[Envelope.ParentIdKey].ShouldEqual(parentId);
            envelope.ParentId.ShouldEqual(parentId);
        }

        [Test]
        public void ResponseId()
        {
            var envelope = new Envelope();
            envelope.ResponseId.ShouldBeNull();

            var responseId = Guid.NewGuid().ToString();
            envelope.ResponseId = responseId;

            envelope.Headers[Envelope.ResponseIdKey].ShouldEqual(responseId);
            envelope.ResponseId.ShouldEqual(responseId);
        }

        [Test]
        public void destination_property()
        {
            var envelope = new Envelope();

            envelope.Destination.ShouldBeNull();

            var uri = "fake://thing".ToUri();
            envelope.Destination = uri;

            envelope.Headers[Envelope.DestinationKey].ShouldEqual(uri.ToString());
            envelope.Destination.ShouldEqual(uri);
        }

        [Test]
        public void reply_requested()
        {
            var envelope = new Envelope();
            envelope.ReplyRequested.ShouldBeFalse();


            envelope.ReplyRequested = true;
            envelope.Headers[Envelope.ReplyRequestedKey].ShouldEqual("true");
            envelope.ReplyRequested.ShouldBeTrue();

            envelope.ReplyRequested = false;
            envelope.Headers.Has(Envelope.ReplyRequestedKey).ShouldBeFalse();
        }
    }
}