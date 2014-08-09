using FubuTransportation.Configuration;
using FubuTransportation.Polling;

namespace FubuTransportation.ScheduledJobs
{
    public class ScheduledJobExpression<T>
    {
        private readonly FubuTransportRegistry _parent;
        private readonly ScheduledJobHandlerSource _scheduledJobs;

        public ScheduledJobExpression(FubuTransportRegistry parent, ScheduledJobHandlerSource scheduledJobs)
        {
            _parent = parent;
            _scheduledJobs = scheduledJobs;
        }

        public ScheduleExpression<TJob> RunJob<TJob>() where TJob : IJob
        {
            return new ScheduleExpression<TJob>(this);
        }

        public class ScheduleExpression<TJob> where TJob : IJob
        {
            private readonly ScheduledJobExpression<T> _parent;

            public ScheduleExpression(ScheduledJobExpression<T> parent)
            {
                _parent = parent;
            }

            public ScheduledJobExpression<T> ScheduledBy<TScheduler>() where TScheduler : IScheduleRule, new()
            {
                return ScheduledBy(new TScheduler());
            }

            public ScheduledJobExpression<T> ScheduledBy(IScheduleRule rule)
            {
                var definition = new ScheduledJob<TJob>(rule);

                _parent._scheduledJobs.JobTypes.Add(typeof(TJob));
                _parent._parent.AlterSettings<ScheduledJobGraph>(x => x.Jobs.Add(definition));

                return _parent;
            }
        }
    }
}