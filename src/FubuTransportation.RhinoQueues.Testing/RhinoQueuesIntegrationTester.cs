using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Transactions;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using NUnit.Framework;
using Rhino.Queues;
using StructureMap;
using System.Linq;

namespace FubuTransportation.RhinoQueues.Testing
{
    [TestFixture]
    public class RhinoQueuesIntegrationTester
    {
        private PersistentQueues queues;
        private RhinoQueuesTransport transport;
        private ChannelGraph graph;
        private ChannelNode node;

        [SetUp]
        public void Setup()
        {
            if(Directory.Exists(PersistentQueues.EsentPath))
                Directory.Delete(PersistentQueues.EsentPath, true);
            if(Directory.Exists("test.esent"))
                Directory.Delete("test.esent", true);

            graph = new ChannelGraph();
            node = graph.ChannelFor<ChannelSettings>(x => x.Upstream);
            node.Uri = new Uri("rhino.queues://localhost:2020/upstream");
            node.Incoming = true;

            queues = new PersistentQueues();
            transport = new RhinoQueuesTransport(queues);

            transport.OpenChannels(graph);
        }


        [Test]
        [Platform(Exclude = "Mono", Reason = "Esent won't work on linux / mono")]
        public void send_a_message_and_get_it_back()
        {
            var envelope = new Envelope(null);
            envelope.Data = new byte[]{1,2,3,4,5};
            envelope.Headers["foo"] = "bar";

            var receiver = new StubReceiver();
            node.Channel.StartReceiving(node, receiver);

            node.Channel.As<RhinoQueuesChannel>().Send(envelope);
            Wait.Until(() => receiver.Received.Any());


            graph.Each(x => x.Channel.Dispose());
            queues.Dispose();

            receiver.Received.Any().ShouldBeTrue();

            var actual = receiver.Received.Single();
            actual.Data.ShouldEqual(envelope.Data);
            actual.Headers["foo"].ShouldEqual("bar");

        }


    }

    public class ChannelSettings
    {
        public Uri Outbound { get; set; }
        public Uri Downstream { get; set; }
        public Uri Upstream { get; set; }
    }

}