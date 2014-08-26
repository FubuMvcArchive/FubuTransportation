using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
            executor.Execute<T>(Timeout);
        }

        public Task<RescheduleRequest<T>> ToTask(IJob job, IJobRunTracker tracker)
        {
            var timeout = new JobTimeout(Timeout);
            return timeout.Execute(job).ContinueWith(t => {
                if (t.IsFaulted)
                {
                    tracker.Failure(t.Exception);
                    throw t.Exception;
                }
                
                var nextTime = Scheduler.ScheduleNextTime(tracker.Now());
                tracker.Success(nextTime);

                return new RescheduleRequest<T>
                {
                    NextTime = nextTime
                };
            });
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

        void IScheduledJob.Initialize(DateTimeOffset now, IJobExecutor executor, JobSchedule schedule)
        {
            var status = schedule.Find(JobType);
            LastExecution = status.LastExecution;

            var next = Scheduler.ScheduleNextTime(now);

            schedule.Schedule(JobType, next);

            executor.Schedule(this, next);
        }
    }
}