using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Timers;
using Bottles;
using Bottles.Diagnostics;
using FubuCore.Descriptions;
using FubuCore.Reflection;
using FubuMVC.Core.Registration;

namespace FubuTransportation.Polling
{
    public interface IJob
    {
        void Execute();
    }

    public class JobRequest<T> where T : IJob{}


    public class JobRunner<T> where T : IJob
    {
        private readonly T _job;

        public JobRunner(T job)
        {
            _job = job;
        }

        public void Run()
        {
            // TODO -- do other things of some sort
            // TODO -- log job completion
            _job.Execute();
        }
    }


    public interface IPollingJob
    {
        bool IsRunning();
        void Start();
        void RunNow();
        void Stop();
        void ResetInterval(double interval);

    }

    // Has to be a singleton
    public interface IPollingJobs : IEnumerable<IPollingJob>
    {

    }

    public class PollingServicesRegistry : ServiceRegistry
    {
        public PollingServicesRegistry()
        {
            // NEED MORE.
            SetServiceIfNone<ITimer, DefaultTimer>();
        }
    }

    public class PollingJobActivator : IActivator
    {
        private readonly IPollingJobs _jobs;

        public PollingJobActivator(IPollingJobs jobs)
        {
            _jobs = jobs;
        }

        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            _jobs.Each(x => {
                try
                {
                    log.Trace("Starting " + Description.For(x).Title);
                    x.Start();
                }
                catch (Exception ex)
                {
                    log.MarkFailure(ex);
                    throw;
                }
            });
        }
    }

    public class PollingJobDeactivator : IDeactivator
    {
        private readonly IPollingJobs _jobs;

        public PollingJobDeactivator(IPollingJobs jobs)
        {
            _jobs = jobs;
        }

        public void Deactivate(IPackageLog log)
        {
            _jobs.Each(x => {
                try
                {
                    x.Stop();
                }
                catch (Exception ex)
                {
                    log.MarkFailure(ex);
                }
            });
        }
    }

    public class PollingJobs : IPollingJobs
    {
        private readonly IEnumerable<IPollingJob> _jobs;

        public PollingJobs(IEnumerable<IPollingJob> jobs)
        {
            _jobs = jobs;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<IPollingJob> GetEnumerator()
        {
            return _jobs.GetEnumerator();
        }
    }

    public class PollingJob<TJob, TSettings> : DescribesItself, IPollingJob where TJob : IJob
    {
        private readonly IServiceBus _bus;
        private readonly ITimer _timer;
        private readonly Expression<Func<TSettings, double>> _intervalSource;
        private double _interval;

        public PollingJob(IServiceBus bus, ITimer timer, TSettings settings, Expression<Func<TSettings, double>> intervalSource)
        {
            _bus = bus;
            _timer = timer;
            _intervalSource = intervalSource;

            _interval = _intervalSource.Compile()(settings);
        }

        public void Describe(Description description)
        {
            description.Title = "Polling Job for " + typeof (TJob).Name;
            typeof(TJob).ForAttribute<DescriptionAttribute>(att => description.ShortDescription = att.Description);
            description.Properties["Interval"] = _interval.ToString();
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
            _bus.Consume(new JobRequest<TJob>());
        }

        public void Stop()
        {
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

    public interface ITimer
    {
        void Start(Action callback, double interval);
        void Restart();
        void Stop();

        bool Enabled { get; }
    }

    public class DefaultTimer : ITimer
    {
        private readonly Timer _timer;
        private Action _callback;

        public DefaultTimer()
        {
            _timer = new Timer { AutoReset = false };
            _timer.Elapsed += elapsedHandler;
        }

        public void Start(Action callback, double interval)
        {
            _callback = callback;

            _timer.Interval = interval;
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Enabled = false;
        }

        public bool Enabled { get { return _timer.Enabled; } }

        public void Restart()
        {
            _timer.Start();
        }

        private void elapsedHandler(object sender, ElapsedEventArgs eventArgs)
        {
            if (_callback == null) return;
            _callback();
        }
    }



}