using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using Bottles;
using Bottles.Diagnostics;
using FubuCore.Descriptions;
using FubuCore.Logging;
using FubuCore.Reflection;
using FubuMVC.Core;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.ObjectGraph;
using System.Linq;
using FubuTransportation.Configuration;

namespace FubuTransportation.Polling
{



    public interface IJob
    {
        void Execute();
    }

    public class JobRequest<T> where T : IJob{}

    public interface IPollingJobLogger
    {
        void Starting(IJob job);
        void Successful(IJob job);
        void Failed(IJob job, Exception ex);
    }

    public class PollingJobSuccess : LogRecord
    {
        public string Description { get; set; }

        protected bool Equals(PollingJobSuccess other)
        {
            return string.Equals(Description, other.Description);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PollingJobSuccess) obj);
        }

        public override int GetHashCode()
        {
            return (Description != null ? Description.GetHashCode() : 0);
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

    public class PollingJobExpression
    {
        private readonly FubuTransportRegistry _parent;

        public PollingJobExpression(FubuTransportRegistry parent)
        {
            _parent = parent;
        }

        public class IntervalExpression<TJob> where TJob : IJob
        {
            private readonly PollingJobExpression _parent;

            public IntervalExpression(PollingJobExpression parent)
            {
                _parent = parent;
            }

            public PollingJobExpression ScheduledAtInterval<TSettings>(
                Expression<Func<TSettings, double>> intervalInMillisecondsProperty)
            {
                var definition = new PollingJobDefinition
                {
                    JobType = typeof (TJob),
                    SettingType = typeof (TSettings),
                    IntervalSource = intervalInMillisecondsProperty
                };

                _parent._parent.AlterSettings<PollingJobSettings>(x => {
                    x.Jobs.Add(definition);
                });

                return _parent;
            }
        }
    }

    [ApplicationLevel]
    public class PollingJobSettings
    {
        public readonly IList<PollingJobDefinition> Jobs = new List<PollingJobDefinition>(); 
    }

    // TODO -- need to register this
    [ConfigurationType(ConfigurationType.Services)]
    public class RegisterPollingJobs : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            var settings = graph.Settings.Get<PollingJobSettings>();
            settings.Jobs.Select(x => x.ToObjectDef()).Each(x => {
                graph.Services.AddService(typeof(IPollingJob), x);
            });
        }
    }

    public class PollingJobDefinition
    {
        public Type JobType { get; set; }
        public Type SettingType { get; set; }
        public Expression IntervalSource { get; set; }

        public ObjectDef ToObjectDef()
        {
            var def = new ObjectDef(typeof (PollingJob<,>), JobType, SettingType);

            var funcType = typeof (Func<,>).MakeGenericType(SettingType, typeof (double));
            var intervalSourceType = typeof (Expression<>).MakeGenericType(funcType);
            def.DependencyByValue(intervalSourceType, IntervalSource);

            return def;
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
            // TODO -- harden & instrument
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
}