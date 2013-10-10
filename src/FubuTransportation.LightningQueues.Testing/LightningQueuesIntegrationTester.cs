using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FubuCore;
using FubuCore.Logging;
using FubuMVC.Core.Runtime.Logging;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Delayed;
using FubuTransportation.Scheduling;
using FubuTransportation.Testing;
using LightningQueues.Model;
using NUnit.Framework;

namespace FubuTransportation.LightningQueues.Testing
{
    [TestFixture]
    public class LightningQueuesIntegrationTester
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
            node.Uri = new Uri("lq.tcp://localhost:2020/upstream");
            node.Incoming = true;

            var delayedCache = new DelayedMessageCache<MessageId>();
            queues = new PersistentQueues(new RecordingLogger(), delayedCache);
            transport = new LightningQueuesTransport(queues, new LightningQueueSettings(), delayedCache);

            transport.OpenChannels(graph);
        }

        [TearDown]
        public void TearDown()
        {
            queues.Dispose();
        }

        private PersistentQueues queues;
        private LightningQueuesTransport transport;
        private ChannelGraph graph;
        private ChannelNode node;


        [Test]
        [Platform(Exclude = "Mono", Reason = "Esent won't work on linux / mono")]
        public void send_a_message_and_get_it_back()
        {
            var envelope = new Envelope() {Data = new byte[] {1, 2, 3, 4, 5}};
            envelope.Headers["foo"] = "bar";

            var receiver = new RecordingReceiver();
            var visitor = new StartingChannelNodeVisitor(receiver);
            visitor.Visit(node);

            node.Channel.As<LightningQueuesChannel>().Send(envelope.Data, envelope.Headers);
            Wait.Until(() => receiver.Received.Any());


            graph.Each(x =>
            {
                var shutdownVisitor = new ShutdownChannelNodeVisitor();
                shutdownVisitor.Visit(x);
            });
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
            var channel = transport.BuildDestinationChannel(new ChannelNode { Uri = new Uri("lq.tcp://localhost:2020/dynamic"), Incoming = true});

            var task = Task.Factory.StartNew(() => channel.Receive(receiver));
            channel.As<LightningQueuesChannel>().Send(envelope.Data, envelope.Headers);
            try
            {
                Wait.Until(() => receiver.Received.Any()).ShouldBeTrue();
            }
            finally
            {
                task.SafeDispose();
                graph.Each(x =>
                {
                    var shutdownVisitor = new ShutdownChannelNodeVisitor();
                    shutdownVisitor.Visit(x);
                });
                queues.Dispose();
            }

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
            replyNode.Uri.ToString().ShouldEqual("lq.tcp://{0}:2020/node/replies".ToFormat(Environment.MachineName.ToLower()));
        }
    }

    public class ChannelSettings
    {
        public Uri Outbound { get; set; }
        public Uri Downstream { get; set; }
        public Uri Upstream { get; set; }
    }
}