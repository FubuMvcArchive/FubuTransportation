using FubuTransportation.Configuration;
using FubuTransportation.Testing.Runtime;
using FubuTransportation.Testing.ScenarioSupport;
using NUnit.Framework;
using FubuMVC.StructureMap;
using StructureMap;
using System.Linq;
using FubuTestingSupport;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class ServiceBus_Consume_right_now_Tester
    {
        [Test]
        public void send_now_is_handled_right_now()
        {
            var container = new Container();
            FubuTransport.For(x => {
                x.Handlers.Include<SimpleHandler<OneMessage>>();
            }).StructureMap(container).Bootstrap();

            var serviceBus = container.GetInstance<IServiceBus>();

            TestMessageRecorder.Clear();

            var message = new OneMessage();

            serviceBus.Consume(message);

            TestMessageRecorder.ProcessedFor<OneMessage>().Single().Message
                               .ShouldBeTheSameAs(message);
        }
    }
}