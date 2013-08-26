using System;
using System.Linq.Expressions;
using FubuTransportation.Configuration;

namespace FubuTransportation.Polling
{
    public class PollingJobExpression
    {
        private readonly FubuTransportRegistry _parent;

        public PollingJobExpression(FubuTransportRegistry parent)
        {
            _parent = parent;
        }

        public IntervalExpression<TJob> RunJob<TJob>() where TJob : IJob
        {
            return new IntervalExpression<TJob>(this);
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

                _parent._parent._pollingJobs.AddJobType(typeof(TJob));
                _parent._parent.AlterSettings<PollingJobSettings>(x => {
                    x.Jobs.Add(definition);
                });

                return _parent;
            }
        }
    }
}