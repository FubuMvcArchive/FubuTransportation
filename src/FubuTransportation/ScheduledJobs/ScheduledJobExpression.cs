using System;
using System.Linq.Expressions;
using FubuCore.Reflection;
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


        public ScheduledJobExpression<T> DefaultJobChannel(Expression<Func<T, object>> channel)
        {
            _parent.AlterSettings<ScheduledJobGraph>(x => x.DefaultChannel = channel.ToAccessor());
            return this;
        } 

        public class ScheduleExpression<TJob> where TJob : IJob
        {
            private readonly ScheduledJobExpression<T> _parent;

            public ScheduleExpression(ScheduledJobExpression<T> parent)
            {
                _parent = parent;
            }



            public ChannelExpression<TJob> ScheduledBy<TScheduler>() where TScheduler : IScheduleRule, new()
            {
                return ScheduledBy(new TScheduler());
            }

            public ChannelExpression<TJob> ScheduledBy(IScheduleRule rule)
            {
                var job = new ScheduledJob<TJob>(rule);

                _parent._scheduledJobs.JobTypes.Add(typeof(TJob));
                _parent._parent.AlterSettings<ScheduledJobGraph>(x => x.Jobs.Add(job));

                return new ChannelExpression<TJob>(job);
            }

            public class ChannelExpression<TJob> where TJob : IJob
            {
                private readonly ScheduledJob<TJob> _job;

                public ChannelExpression(ScheduledJob<TJob> job)
                {
                    _job = job;
                }

                public ChannelExpression<TJob> Channel(Expression<Func<T, object>> channel)
                {
                    _job.Channel = channel.ToAccessor();
                    return this;
                }

                public ChannelExpression<TJob> Timeout(TimeSpan timeout)
                {
                    _job.Timeout = timeout;
                    return this;
                }
            }
        }
    }
}