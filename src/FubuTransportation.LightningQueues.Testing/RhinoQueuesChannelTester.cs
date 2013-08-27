using System;
using System.Collections.Specialized;
using FubuTestingSupport;
using FubuTransportation.Runtime;
using LightningQueues.Model;
using NUnit.Framework;

namespace FubuTransportation.LightningQueues.Testing
{
    [TestFixture]
    public class when_creating_an_envelope_from_a_lightning_queue_message
    {
        private Message message;
        private Envelope theEnvelope;

        [SetUp]
        public void SetUp()
        {
            message = new Message
            {
                Data = new byte[] {1, 2, 3, 4},
                Headers = new NameValueCollection(),
                Id = new MessageId {MessageIdentifier = Guid.NewGuid()}
            };

            message.Headers["a"] = "1";
            message.Headers["b"] = "2";
            message.Headers["c"] = "3";

            theEnvelope = message.ToEnvelope();
        }

        [Test]
        public void should_copy_the_data()
        {
            theEnvelope.Data.ShouldEqual(message.Data);
        }

        [Test]
        public void should_copy_the_headers()
        {
            theEnvelope.Headers["a"].ShouldEqual("1");
            theEnvelope.Headers["b"].ShouldEqual("2");
            theEnvelope.Headers["c"].ShouldEqual("3");
        }

    }
}