using System.Reflection;
using FubuTransportation.Configuration;
using FubuTransportation.Testing.ScenarioSupport;
using NUnit.Framework;
using FubuTestingSupport;
using FubuMVC.StructureMap;
using StructureMap;

namespace FubuTransportation.Testing.Configuration
{
    [TestFixture]
    public class FubuTransportRegistryTester
    {
        [Test]
        public void find_the_calling_assembly()
        {
            FubuTransportRegistry.FindTheCallingAssembly()
                                 .ShouldEqual(Assembly.GetExecutingAssembly());
        }

        [Test]
        public void able_to_derive_the_node_name_from_fubu_transport_registry_name()
        {
            var runtime = FubuTransport.For<CustomTransportRegistry>().StructureMap(new Container()).Bootstrap();
            runtime.Factory.Get<ChannelGraph>().Name.ShouldEqual("custom");

            FubuTransport.For<OtherRegistry>().StructureMap(new Container()).Bootstrap()
                         .Factory.Get<ChannelGraph>().Name.ShouldEqual("other");
        }

        [Test]
        public void can_set_the_node_name_programmatically()
        {
            FubuTransport.For(x => x.NodeName = "MyNode").StructureMap(new Container()).Bootstrap()
                         .Factory.Get<ChannelGraph>().Name.ShouldEqual("MyNode");
        }
    }

    public class CustomTransportRegistry : FubuTransportRegistry
    {
        public CustomTransportRegistry()
        {
           
        }
    }

    public class OtherRegistry : FubuTransportRegistry
    {
        public OtherRegistry()
        {
        }
    }
}