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
    }
}