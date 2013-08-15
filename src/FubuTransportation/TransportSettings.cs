using FubuMVC.Core.Registration;

namespace FubuTransportation
{
    [ApplicationLevel]
    public class TransportSettings
    {
        public TransportSettings()
        {
            DebugEnabled = false;
        }

        public bool DebugEnabled { get; set; }
    }
}