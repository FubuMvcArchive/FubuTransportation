using System;

namespace FubuTransportation.Monitoring
{
    public class HealthMonitoringSettings
    {
        public int Seed
        {
            set { Random = new Random(value * 1000); }
        }

        public Random Random = new Random(60000);

        public double Interval
        {
            get { return Random.NextDouble(); }
        }
    }
}