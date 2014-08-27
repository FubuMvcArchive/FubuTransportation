using System;

namespace FubuTransportation.Monitoring
{
    public class TaskStatus
    {
        public Uri Subject { get; set; }
        public HealthStatus Status { get; set; }
    }
}