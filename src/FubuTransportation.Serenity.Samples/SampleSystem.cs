using FubuTransportation.Configuration;
using FubuTransportation.Serenity.Samples.SystemUnderTest;

namespace FubuTransportation.Serenity.Samples
{
    public class SampleSystem : FubuTransportSystem<TestApplication>
    {
        public SampleSystem()
        {
            FubuTransport.SetupForInMemoryTesting();
            OnContextCreation<MessageRecorder>(x => x.Messages.Clear());
        }
    }
}