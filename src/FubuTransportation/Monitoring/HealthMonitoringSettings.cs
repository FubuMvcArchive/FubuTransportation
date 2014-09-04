using System;

namespace FubuTransportation.Monitoring
{
    public class HealthMonitoringSettings
    {
        private bool _initial = true;

        public int Seed
        {
            set { Random = new Random(value * 1000); }
        }

        public Random Random = new Random(60000);

        public double Interval
        {
            get
            {
                if (_initial)
                {
                    _initial = false;
                    return 100;
                }
                
                return Random.NextDouble();
            }
        }
    }
}