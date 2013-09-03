using System;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Testing.Runtime;
using FubuTransportation.Testing.ScenarioSupport;
using NUnit.Framework;
using Rhino.Mocks;
using FubuCore;
using System.Linq;
using Message1 = FubuTransportation.Testing.Events.Message1;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class ServiceBus_DelaySend_Tester : InteractionContext<ServiceBus>
    {
        protected override void beforeEach()
        {
            LocalSystemTime = DateTime.Today.AddHours(4);

            
        }

        private Envelope theLastEnvelopeSent
        {
            get
            {
                return MockFor<IEnvelopeSender>().GetArgumentsForCallsMadeOn(x => x.Send(null))
                                                 .Last()[0].As<Envelope>();
            }
        }

        [Test]
        public void send_by_a_time()
        {
            var expectedTime = DateTime.Today.AddHours(5);
            var theMessage = new Message1();
            ClassUnderTest.DelaySend(theMessage, expectedTime);

            theLastEnvelopeSent.Message.ShouldBeTheSameAs(theMessage);
            theLastEnvelopeSent.ExecutionTime.ShouldEqual(expectedTime.ToUniversalTime());
        }

        [Test]
        public void send_by_delay()
        {
            var theMessage = new Message1();
            ClassUnderTest.DelaySend(theMessage, 5.Hours());

            theLastEnvelopeSent.Message.ShouldBeTheSameAs(theMessage);
            theLastEnvelopeSent.ExecutionTime.ShouldEqual(UtcSystemTime.AddHours(5));
        }
    }
}