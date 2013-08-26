using System;
using System.ComponentModel;
using System.Linq.Expressions;
using FubuCore.Descriptions;
using FubuCore.Reflection;

namespace FubuTransportation.Polling
{
    public class PollingJob<TJob, TSettings> : DescribesItself, IPollingJob where TJob : IJob
    {
        private readonly IServiceBus _bus;
        private readonly IPollingJobLogger _logger;
        private readonly ITimer _timer;
        private readonly Expression<Func<TSettings, double>> _intervalSource;
        private double _interval;

        public PollingJob(IServiceBus bus, IPollingJobLogger logger, TSettings settings, Expression<Func<TSettings, double>> intervalSource)
        {
            _bus = bus;
            _logger = logger;
            _timer = new DefaultTimer();
            _intervalSource = intervalSource;

            _interval = _intervalSource.Compile()(settings);
        }

        public void Describe(Description description)
        {
            description.Title = "Polling Job for " + typeof (TJob).Name;
            typeof(TJob).ForAttribute<DescriptionAttribute>(att => description.ShortDescription = att.Description);
            description.Properties["Interval"] = _interval.ToString() + " ms";
            description.Properties["Config"] = _intervalSource.ToString();
        }

        public bool IsRunning()
        {
            return _timer.Enabled;
        }

        public void Start()
        {
            _timer.Start(RunNow, _interval);
        }

        public void RunNow()
        {
            try
            {
                _bus.Consume(new JobRequest<TJob>());
            }
            catch (Exception e)
            {
                _logger.FailedToSchedule(typeof (TJob), e);
            }
        }

        public void Stop()
        {
            _logger.Stopping(typeof(TJob));
            _timer.Stop();
        }

        public void ResetInterval(double interval)
        {
            _interval = interval;
            
            if (_timer.Enabled)
            {
                Stop();
                Start();
            }
        }

    }
}