using System.IO;
using System.Linq;
using System.Net;
using FubuCore.Logging;
using FubuMVC.Core.Runtime.Logging;
using FubuTestingSupport;
using FubuTransportation.Runtime.Delayed;
using LightningQueues.Model;
using NUnit.Framework;

namespace FubuTransportation.LightningQueues.Testing
{
    [TestFixture]
    public class PersistentQueueTester
    {
        [SetUp]
        public void Setup()
        {
            if (Directory.Exists("fubutransportation.esent"))
                Directory.Delete("fubutransportation.esent", true);
        }

        [Test]
        [Platform(Exclude = "Mono", Reason = "Esent won't work on linux / mono")]
        public void creates_queues_when_started()
        {
            using (var queues = new PersistentQueues(new RecordingLogger(), new DelayedMessageCache<MessageId>()))
            {
                queues.Start(new LightningUri[]
                {
                    new LightningUri("lq.tcp://localhost:2424/some_queue"), 
                    new LightningUri("lq.tcp://localhost:2424/other_queue"), 
                    new LightningUri("lq.tcp://localhost:2424/third_queue"), 
                });

                queues.ManagerFor(new IPEndPoint(IPAddress.Loopback, 2424))
                    .Queues.OrderBy(x => x).ShouldHaveTheSameElementsAs(LightningQueuesTransport.DelayedQueueName,"other_queue", "some_queue", "third_queue");
            }
        }
    }
}