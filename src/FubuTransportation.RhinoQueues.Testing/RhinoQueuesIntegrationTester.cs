using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading;
using System.Transactions;
using FubuMVC.Core;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using NUnit.Framework;
using Rhino.Queues;
using StructureMap;

namespace FubuTransportation.RhinoQueues.Testing
{
    [TestFixture]
    public class RhinoQueuesIntegrationTester
    {
        [SetUp]
        public void Setup()
        {
            if(Directory.Exists("fubutransportation.esent"))
                Directory.Delete("fubutransportation.esent", true);
            if(Directory.Exists("test.esent"))
                Directory.Delete("test.esent", true);
        }

        [Test]
        [Platform(Exclude = "Mono", Reason = "Esent won't work on linux / mono")]
        public void can_receive_messages()
        {
            var sender = new QueueManager(new IPEndPoint(IPAddress.Loopback, 2201), "test.esent");
            sender.Start();

            var container = new Container();
            var settings = new RhinoQueuesSettings()
                .AddQueue("testqueue");
            container.Configure(x => x.For<RhinoQueuesSettings>().Use(settings));
            FubuTransport.For<RQTransportRegistry>()
                .StructureMap(container)
                .Bootstrap();

            var handle = TestConsumer.WaitHandle = new ManualResetEvent(false);
            var serializer = new XmlMessageSerializer();
            var message = new MessagePayload()
            {
                Headers = new NameValueCollection()
            };

            message.Headers[Envelope.ContentTypeKey] = serializer.ContentType;

            using (var ms = new MemoryStream())
            {
                serializer.Serialize(new object[]{new TestMessage()}, ms);
                message.Data = ms.ToArray();
            }
            using (var tx = new TransactionScope())
            {
                sender.Send(new Uri("rhino.queues://localhost:2200/testqueue"), message);
                tx.Complete();
            }

            handle.WaitOne(TimeSpan.FromSeconds(3)).ShouldBeTrue();
            sender.Dispose();
        }
    }

    public class RQTransportRegistry : FubuTransportRegistry
    {
    }

    public class TestConsumer
    {
        //TODO fragile, maybe pull out to a wrapped invoker or something rather than static state
        public static ManualResetEvent WaitHandle;

        public void Consume(TestMessage message)
        {
            WaitHandle.Set();
        }
    }

    public class TestMessage
    {
    }
}