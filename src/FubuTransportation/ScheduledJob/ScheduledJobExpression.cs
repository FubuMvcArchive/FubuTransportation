using FubuTransportation.Configuration;
using FubuTransportation.Polling;
using FubuTransportation.Scheduling;

namespace FubuTransportation.ScheduledJob
{
    public class ScheduledJobExpression
    {
        private readonly FubuTransportRegistry _parent;

        public ScheduledJobExpression(FubuTransportRegistry parent)
        {
            _parent = parent;
        }

        public ScheduleExpression<TJob> RunJob<TJob>() where TJob : IJob
        {
            return new ScheduleExpression<TJob>(this);
        } 

        public class ScheduleExpression<TJob> where TJob : IJob
        {
            private readonly ScheduledJobExpression _parent;

            public ScheduleExpression(ScheduledJobExpression parent)
            {
                _parent = parent;
            }

            public ScheduledJobExpression ScheduledBy<TScheduler>() where TScheduler : IScheduleRule, new()
            {
                return ScheduledBy(new TScheduler());
            }

            public ScheduledJobExpression ScheduledBy(IScheduleRule scheduler)
            {
                var definition = new ScheduledJobDefinition
                {
                    JobType = typeof(TJob),
                    Scheduler = scheduler
                };

                _parent._parent._scheduledJobs.AddJobType(typeof(TJob));
                _parent._parent.AlterSettings<ScheduledJobSettings>(x => x.Jobs.Add(definition));
                return _parent;
            }
        }
    }
}