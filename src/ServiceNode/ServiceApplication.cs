using FubuMVC.Core;
using FubuMVC.StructureMap;
using FubuTransportation.Configuration;

namespace ServiceNode
{
    public class ServiceApplication : IApplicationSource
    {
        public FubuApplication BuildApplication()
        {
            return FubuTransport.For<ServiceRegistry>().StructureMap();
        }
    }

    public class ServiceRegistry : FubuTransportRegistry<TestBusSettings>
    {
        public ServiceRegistry()
        {
            Channel(x => x.Service).ReadIncoming();
        }
    }
}