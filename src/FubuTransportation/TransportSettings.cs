using FubuMVC.Core.Registration;

namespace FubuTransportation
{
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