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

        /// <summary>
        /// Use this to identify the running node of FubuTransportation
        /// </summary>
        public string Name { get; set; }
        public bool DebugEnabled { get; set; }
        public bool InfoEnabled { get; set; }
    }
}