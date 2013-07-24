using System.IO;
using FubuTestingSupport;
using NUnit.Framework;

namespace FubuTransportation.RhinoQueues.Testing
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
            var setting = new RhinoQueuesSettings();
            setting.Queues.Add(new QueueSetting{QueueName = "test", ThreadCount = 1});
            using (var queue = new PersistentQueue(setting))
            {
                queue.Start();
                queue.QueueManager.Queues[0].ShouldEqual("test");
            }
        }
    }
}