using FubuTransportation.Serenity.Samples.SystemUnderTest;

namespace FubuTransportation.Serenity.Samples
{
    public class SampleSystem : FubuTransportSystem<TestApplication>
    {
        public SampleSystem()
        {
            OnContextCreation<MessageRecorder>(x => x.Messages.Clear());

            TestRegistry.InMemory = true;
            TestNodes.OnNodeCreation(registry => registry.EnableInMemoryTransport());
        }
    }
}