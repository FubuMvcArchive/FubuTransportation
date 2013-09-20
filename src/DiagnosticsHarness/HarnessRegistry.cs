using FubuTransportation.Configuration;

namespace DiagnosticsHarness
{
    public class HarnessRegistry : FubuTransportRegistry<HarnessSettings>
    {
        public HarnessRegistry()
        {
            // TODO -- publish everything option in the FI?
            Channel(x => x.Channel).ReadIncoming().PublishesMessages(x => true);
        }
    }
}