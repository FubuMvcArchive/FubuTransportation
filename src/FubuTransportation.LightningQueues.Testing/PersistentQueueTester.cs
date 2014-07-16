using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Transactions;
using FubuCore.Logging;
using FubuTestingSupport;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Delayed;
using LightningQueues;
using LightningQueues.Model;
using NUnit.Framework;

namespace FubuTransportation.LightningQueues.Testing
{
    [TestFixture]
    public class PersistentQueueTester
    {
        [Test]
        [Platform(Exclude = "Mono", Reason = "Esent won't work on linux / mono")]
        public void creates_queues_when_started()
        {
            using (var queues = new PersistentQueues(new RecordingLogger(), new DelayedMessageCache<MessageId>()))
            {
                queues.ClearAll();
                queues.Start(new LightningUri[]
                {
                    new LightningUri("lq.tcp://localhost:2424/some_queue"), 
                    new LightningUri("lq.tcp://localhost:2424/other_queue"), 
                    new LightningUri("lq.tcp://localhost:2424/third_queue"), 
                });

                queues.ManagerFor(2424, true)
                    .Queues.OrderBy(x => x).ShouldHaveTheSameElementsAs(LightningQueuesTransport.DelayedQueueName, LightningQueuesTransport.ErrorQueueName, "other_queue", "some_queue", "third_queue");
            }
        }

        [Test]
        [Platform(Exclude = "Mono", Reason = "Esent won't work on linux / mono")]
        public void recovers_delayed_messages_when_started()
        {
            using (var queues = new PersistentQueues(new RecordingLogger(), new DelayedMessageCache<MessageId>()))
            {
                queues.ClearAll();
                queues.Start(new []{ new LightningUri("lq.tcp://localhost:2425/the_queue") });

                var envelope = new Envelope();
                envelope.Data = new byte[0];
                envelope.ExecutionTime = DateTime.UtcNow;
                var delayedMessage = new MessagePayload
                {
                    Data = envelope.Data,
                    Headers = envelope.Headers.ToNameValues()
                };

                using (var scope = new TransactionScope())
                {
                    queues.ManagerFor(2425, true)
                        .EnqueueDirectlyTo(LightningQueuesTransport.DelayedQueueName, null, delayedMessage); 
                    scope.Complete();
                }
            }

            var cache = new DelayedMessageCache<MessageId>();
            using (var queues = new PersistentQueues(new RecordingLogger(), cache))
            {
                queues.Start(new []{ new LightningUri("lq.tcp://localhost:2425/the_queue") });

                cache.AllMessagesBefore(DateTime.UtcNow.AddSeconds(1)).ShouldNotBeEmpty();
            }
        }
    }
}