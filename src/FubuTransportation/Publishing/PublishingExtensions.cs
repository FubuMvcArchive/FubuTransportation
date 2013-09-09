using FubuMVC.Core;

namespace FubuTransportation.Publishing
{

    public class PublishingExtensions : IFubuRegistryExtension
    {
        public void Configure(FubuRegistry registry)
        {
            registry.Actions.FindWith<EventPublishingActionSource>();
            registry.Policies.Add<EventPublisherConvention>();
        }
    }
}