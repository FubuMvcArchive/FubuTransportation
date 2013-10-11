using FubuTransportation.Configuration;

namespace FubuTransportation.Testing.Docs.GettingStarted
{
    // SAMPLE: GettingStartedTransportRegistry
    public class GettingStartedTransportRegistry : FubuTransportRegistry<GettingStartedSettings>
    {
        public GettingStartedTransportRegistry()
        {
            Channel(x => x.Uri)
                //Routes messages in the in the getting started namespace to this channel
                .AcceptsMessages(x => typeof(GettingStartedSettings).Namespace.Equals(x.Namespace))
                .ReadIncoming();
        }
    }
    // ENDSAMPLE
}