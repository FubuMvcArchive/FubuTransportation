using System;
using FubuCore;
using FubuCore.Reflection;
using FubuTransportation.Polling;
using FubuTransportation.Runtime.Routing;

namespace FubuTransportation.ScheduledJobs
{
    public class ScheduledJob<T> : IScheduledJob, IScheduledJob<T> where T : IJob
    {
        public ScheduledJob(IScheduleRule scheduler)
        {
            Scheduler = scheduler;
            Timeout = 5.Minutes();
        }

        // TODO -- add timeouts?
        // This will be completely tested through integration
        // tests only
        void IScheduledJob<T>.Execute(IJobExecutor executor)
        {
            executor.Execute<T>()
                .ContinueWith(task => Reschedule(task.Result, executor));
        }

        public void Reschedule(JobExecutionRecord record, IJobExecutor executor)
        {
            if (record.Success)
            {
                LastExecution = record;
                var next = Scheduler.ScheduleNextTime(executor.Now());

                executor.Schedule(this, next);
            }
            else
            {
                // TODO -- this won't be like this in the long run
                LastExecution = record;
                var next = Scheduler.ScheduleNextTime(executor.Now());

                executor.Schedule(this, next);
            }
        }

        public Accessor Channel { get; set; }
        public TimeSpan Timeout { get; set; }

        IRoutingRule IScheduledJob.ToRoutingRule()
        {
            return new ScheduledJobRoutingRule<T>();
        }

        public Type JobType
        {
            get { return typeof (T); }
        }

        public IScheduleRule Scheduler { get; private set; }
        public JobExecutionRecord LastExecution { get; set; }

        void IScheduledJob.Initialize(IJobExecutor executor, JobSchedule schedule)
        {
            var status = schedule.Find(JobType);
            LastExecution = status.LastExecution;

            var next = Scheduler.ScheduleNextTime(executor.Now());

            schedule.Schedule(JobType, next);

            executor.Schedule(this, next);
        }
    }
}