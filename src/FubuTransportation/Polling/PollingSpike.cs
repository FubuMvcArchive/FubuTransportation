using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FubuCore.Logging;
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
        void FailedToSchedule(Type jobType, Exception exception);
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

    public interface IPollingJobs : IEnumerable<IPollingJob>
    {

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
}