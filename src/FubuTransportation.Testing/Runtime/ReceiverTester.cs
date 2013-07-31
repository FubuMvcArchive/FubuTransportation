using System;
using System.Collections.Generic;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Runtime
{
    [TestFixture]
    public class ReceiverContentTypeHandling
    {
        private ChannelGraph theGraph;
        private ChannelNode theNode;
        private RecordingMessageInvoker theInvoker;
        private Receiver theReceiver;

        [SetUp]
        public void SetUp()
        {
            theGraph = new ChannelGraph();
            theNode = new ChannelNode();

            theInvoker = new RecordingMessageInvoker();

            theReceiver = new Receiver(theInvoker, theGraph, theNode);
        }

        [Test]
        public void if_no_content_type_is_specified_on_envelope_or_channel_use_graph_default()
        {
            theGraph.DefaultContentType = "text/json";
            theNode.DefaultContentType = null;
            var envelope = new Envelope(null);
            envelope.ContentType.ShouldBeNull();

            theReceiver.Receive(envelope);

            envelope.ContentType.ShouldEqual("text/json");
        }

        [Test]
        public void if_no_content_type_is_specified_use_channel_default_when_it_exists()
        {
            theGraph.DefaultContentType = "text/json";
            theNode.DefaultContentType = "text/xml";

            var envelope = new Envelope(null);
            envelope.ContentType.ShouldBeNull();

            theReceiver.Receive(envelope);

            envelope.ContentType.ShouldEqual("text/xml");
        }

        [Test]
        public void the_envelope_content_type_wins()
        {
            theGraph.DefaultContentType = "text/json";
            theNode.DefaultContentType = "text/xml";

            var envelope = new Envelope(null);
            envelope.ContentType = "text/plain";

            theReceiver.Receive(envelope);

            envelope.ContentType.ShouldEqual("text/plain");
        }
    }

    public class RecordingMessageInvoker : IMessageInvoker
    {
        public IList<Envelope> Invoked = new List<Envelope>();

        public void Invoke(Envelope envelope)
        {
            Invoked.Add(envelope);
        }
    }


    [TestFixture]
    public class when_receiving_a_message : InteractionContext<Receiver>
    {
        Envelope envelope = new Envelope(null);
        Uri address = new Uri("foo://bar");
        private IChannel theChannel;
        private ChannelNode theNode;

        protected override void beforeEach()
        {
            theChannel = MockFor<IChannel>();
            theChannel.Stub(x => x.Address).Return(address);

            theNode = new ChannelNode
            {
                DefaultContentType = "application/json",
                Channel = theChannel,
                Uri = address
            };

            Services.Inject(theNode);
            
            ClassUnderTest.Receive(envelope);
        }

        [Test]
        public void should_copy_the_channel_address_to_the_envelope()
        {
            envelope.Source.ShouldEqual(address);
        }

        [Test]
        public void should_call_through_to_the_invoker()
        {
            MockFor<IMessageInvoker>().AssertWasCalled(x => x.Invoke(envelope));
        }
    }
}