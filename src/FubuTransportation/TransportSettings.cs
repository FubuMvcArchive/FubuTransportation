using FubuMVC.Core.Registration;

namespace FubuTransportation
{
    // TODO -- is this an app level thing, or does it need to be coming from config?
    [ApplicationLevel]
    public class TransportSettings
    {
        public TransportSettings()
        {
            DebugEnabled = false;
            InfoEnabled = true;
        }

        public bool DebugEnabled { get; set; }
        public bool InfoEnabled { get; set; }
    }
}