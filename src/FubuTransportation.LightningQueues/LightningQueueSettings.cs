namespace FubuTransportation.LightningQueues
{
    public class LightningQueueSettings
    {
        public LightningQueueSettings()
        {
            DefaultPort = 2020;
        }

        public bool Disabled { get; set; }
        public int DefaultPort { get; set; } 

        /// <summary>
        /// Setting this flag to "true" will disable
        /// the LightningQueues transport if there
        /// are no LightningQueues channels
        /// </summary>
        public bool DisableIfNoChannels { get; set; }
    }
}