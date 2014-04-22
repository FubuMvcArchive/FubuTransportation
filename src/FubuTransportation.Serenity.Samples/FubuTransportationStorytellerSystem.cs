using FubuTransportation.Configuration;
using WebsiteNode;

namespace FubuTransportation.Serenity.Samples
{
    public class SampleSystem : FubuTransportSystem<WebsiteApplication>
    {
        public SampleSystem()
        {
            FubuTransport.SetupForInMemoryTesting();
        }
    }
}