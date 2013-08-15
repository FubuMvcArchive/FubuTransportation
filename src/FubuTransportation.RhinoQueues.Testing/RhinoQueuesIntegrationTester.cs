using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Testing;
using NUnit.Framework;

namespace FubuTransportation.RhinoQueues.Testing
{
    [TestFixture]
    public class RhinoQueuesIntegrationTester
    {
        [SetUp]
        public void Setup()
        {
            if (Directory.Exists(PersistentQueues.EsentPath))
                Directory.Delete(PersistentQueues.EsentPath, true);
            if (Directory.Exists("test.esent"))
                Directory.Delete("test.esent", true);

            graph = new ChannelGraph();
            node = graph.ChannelFor<ChannelSettings>(x => x.Upstream);
            node.Uri = new Uri("rhino.queues://localhost:2020/upstream");
            node.Incoming = true;

            queues = new PersistentQueues();
            transport = new RhinoQueuesTransport(queues, new RhinoQueueSettings());

            transport.OpenChannels(graph);
        }

        [TearDown]
        public void TearDown()
        {
            queues.Dispose();
        }

        private PersistentQueues queues;
        private RhinoQueuesTransport transport;
        private ChannelGraph graph;
        private ChannelNode node;


        [Test]
        [Platform(Exclude = "Mono", Reason = "Esent won't work on linux / mono")]
        public void send_a_message_and_get_it_back()
        {
            var envelope = new Envelope() {Data = new byte[] {1, 2, 3, 4, 5}};
            envelope.Headers["foo"] = "bar";

            var receiver = new RecordingReceiver();
            node.Channel.StartReceiving(receiver, new ChannelNode());

            node.Channel.As<RhinoQueuesChannel>().Send(envelope.Data, envelope.Headers);
            Wait.Until(() => receiver.Received.Any());


            graph.Each(x => x.Channel.Dispose());
            queues.Dispose();

            receiver.Received.Any().ShouldBeTrue();

            Envelope actual = receiver.Received.Single();
            actual.Data.ShouldEqual(envelope.Data);
            actual.Headers["foo"].ShouldEqual("bar");
        }


        [Test]
        [Platform(Exclude = "Mono", Reason = "Esent won't work on linux / mono")]
        public void send_a_message_and_get_it_back_for_queue_built_at_runtime()
        {
            var envelope = new Envelope() { Data = new byte[] { 1, 2, 3, 4, 5 } };
            envelope.Headers["foo"] = "bar";

            var receiver = new RecordingReceiver();
            var channel = transport.BuildChannel(new ChannelNode { Uri = new Uri("rhino.queues://localhost:2020/dynamic") });

            channel.StartReceiving(receiver, new ChannelNode());
            channel.As<RhinoQueuesChannel>().Send(envelope.Data, envelope.Headers);
            Wait.Until(() => receiver.Received.Any());


            graph.Each(x => x.Channel.Dispose());
            queues.Dispose();

            receiver.Received.Any().ShouldBeTrue();

            Envelope actual = receiver.Received.Single();
            actual.Data.ShouldEqual(envelope.Data);
            actual.Headers["foo"].ShouldEqual("bar");
        }

        [Test]
        [Platform(Exclude = "Mono", Reason = "Esent won't work on linux / mono")]
        public void can_find_the_reply_channel()
        {
            var subscriptions = new Subscriptions(graph, () => null, new ITransport[0]);
            var replyNode = subscriptions.ReplyNodeFor(node);
            replyNode.ShouldNotBeNull();
            replyNode.Uri.ToString().ShouldEqual("rhino.queues://{0}:2020/node/replies".ToFormat(Environment.MachineName.ToLower()));
        }
    }

    public class ChannelSettings
    {
        public Uri Outbound { get; set; }
        public Uri Downstream { get; set; }
        public Uri Upstream { get; set; }
    }
}