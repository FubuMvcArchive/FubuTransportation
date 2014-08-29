using System;
using System.Linq;
using FubuCore;
using FubuTestingSupport;
using FubuTransportation.Runtime;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class ServiceBus_Send_to_destination_Tester : InteractionContext<ServiceBus>
    {
        private Envelope theLastEnvelopeSent
        {
            get
            {
                return MockFor<IEnvelopeSender>().GetArgumentsForCallsMadeOn(x => x.Send(null))
                    .Last()[0].As<Envelope>();
            }
        }

        [Test]
        public void sends_to_appropriate_destination()
        {
            var destination = new Uri("memory://blah");
            var message = new Message1();
            
            ClassUnderTest.Send(destination, message);

            theLastEnvelopeSent.Destination.ShouldEqual(destination);
            theLastEnvelopeSent.Message.ShouldBeTheSameAs(message);
        }
    }
}