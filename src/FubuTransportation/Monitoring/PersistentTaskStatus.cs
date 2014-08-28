using System;

namespace FubuTransportation.Monitoring
{
    public class PersistentTaskStatus
    {
        public Uri Subject { get; set; }
        public HealthStatus Status { get; set; }
    }
}