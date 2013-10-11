using FubuMVC.Core;
using FubuMVC.StructureMap;
using FubuTransportation.Configuration;
using ServiceNode;

namespace WebsiteNode
{
    public class WebsiteApplication : IApplicationSource
    {
        public FubuApplication BuildApplication()
        {
            return FubuTransport.For<WebsiteRegistry>().StructureMap();
        }
    }

    public class WebsiteRegistry : FubuTransportRegistry<TestBusSettings>
    {
        public WebsiteRegistry()
        {
            Channel(x => x.Website).ReadIncoming();
            Channel(x => x.Service).PublishesMessagesInAssemblyContainingType<ServiceApplication>();
        }
    }
}