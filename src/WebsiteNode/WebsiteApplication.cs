using FubuMVC.Core;
using FubuTransportation.Configuration;
using ServiceNode;

namespace WebsiteNode
{
    public class WebsiteApplication : IApplicationSource
    {
        public FubuApplication BuildApplication()
        {
            throw new System.NotImplementedException();
        }
    }

    public class WebsiteRegistry : FubuTransportRegistry<TestBusSettings>
    {
        protected WebsiteRegistry()
        {
            Channel(x => x.Website).ReadIncoming();
            Channel(x => x.Service).PublishesMessagesInAssemblyContainingType<ServiceApplication>();
        }
    }
}