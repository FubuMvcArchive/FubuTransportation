using System;
using System.Threading;
using FubuTransportation.Polling;

namespace FubuTransportation.Monitoring
{
    public class HealthMonitoringSettings
    {
        public int Seed
        {
            set { Random = new Random(value); }
        }

        public Random Random = new Random(60000);

        public double Interval
        {
            get { return Random.NextDouble(); }
        }
    }

    public class HealthMonitorPollingJob : IJob
    {
        private readonly IPersistentTaskController _controller;

        public HealthMonitorPollingJob(IPersistentTaskController controller)
        {
            _controller = controller;
        }

        public void Execute(CancellationToken cancellation)
        {
            _controller.EnsureTasksHaveOwnership();
        }
    }
}