using FubuTransportation.Configuration;
using FubuTransportation.Runtime.Delayed;

namespace FubuTransportation.Polling
{
    public class BuiltInPollingJobRegistry : FubuTransportRegistry
    {
        public BuiltInPollingJobRegistry() 
        {
            Handlers.DisableDefaultHandlerSource();
            Polling.RunJob<DelayedEnvelopeProcessor>()
                   .ScheduledAtInterval<TransportSettings>(x => x.DelayMessagePolling);
        }
    }
}