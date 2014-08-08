using FubuTransportation.Configuration;
using FubuTransportation.Polling;

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

            public ScheduledJobExpression ScheduledBy<TScheduler>() where TScheduler : IJobScheduler
            {
                var definition = new ScheduledJobDefinition
                {
                    JobType = typeof(TJob),
                    SchedulerType = typeof(TScheduler)
                };

                _parent._parent._scheduledJobs.AddJobType(typeof(TJob));
                _parent._parent.AlterSettings<ScheduledJobSettings>(x => x.Jobs.Add(definition));
                return _parent;
            }
        }
    }
}