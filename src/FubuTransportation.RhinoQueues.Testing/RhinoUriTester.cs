using System;
using System.Net;
using FubuTestingSupport;
using NUnit.Framework;

namespace FubuTransportation.RhinoQueues.Testing
{
    [TestFixture]
    public class RhinoUriTester
    {
        private RhinoUri theUri;

        [SetUp]
        public void SetUp()
        {
            theUri = new RhinoUri("rhino.queues://localhost:2424/some_queue");
        }

        [Test]
        public void blows_up_if_protocol_is_not_rhino_queues()
        {
            Exception<ArgumentOutOfRangeException>.ShouldBeThrownBy(() => {
                new RhinoUri("foo://bar");
            });
        }

        [Test]
        public void finds_the_port()
        {
            theUri.Port.ShouldEqual(2424);
        }

        [Test]
        public void parses_the_queue_name()
        {
            theUri.QueueName.ShouldEqual("some_queue");
        }

        [Test]
        public void builds_the_IpEndpoint_for_local()
        {
            theUri.Endpoint.ShouldEqual(new IPEndPoint(IPAddress.Loopback, 2424));
        }

        [Test]
        public void builds_IPEndpoint_for_non_local()
        {
            var rUri = new RhinoUri("rhino.queues://1.5.2.5:2000/some_queue");
            rUri.Endpoint.ShouldEqual(new IPEndPoint(IPAddress.Parse("1.5.2.5"), 2000));
        }
    }
}