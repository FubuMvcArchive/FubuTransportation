using FubuTransportation.Configuration;
using FubuTransportation.Events;
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

            Polling.RunJob<ExpiringListenerCleanup>()
                .ScheduledAtInterval<TransportSettings>(x => x.ListenerCleanupPolling);
        }
    }
}