using System;
using FubuCore.Reflection;
using FubuTransportation.Polling;
using FubuTransportation.Runtime.Routing;

namespace FubuTransportation.ScheduledJobs
{
    public interface IScheduledJob
    {
        Type JobType { get; }
        IScheduleRule Scheduler { get; }
        void Reschedule(DateTimeOffset now, JobSchedule schedule);
        Accessor Channel { get; }

        IRoutingRule ToRoutingRule();
        void RegisterJob(IJobTimer timer, IJobExecutor executor, JobStatus status);
    }

    public interface IScheduledJob<T> : IScheduledJob where T : IJob
    {
        JobStatus ToNewJobStatus(JobExecutionRecord record, DateTimeOffset now);
    }

    public class ScheduledJob<T> : IScheduledJob<T> where T : IJob
    {
        // TODO -- need to add channel here.

        public ScheduledJob(IScheduleRule scheduler)
        {
            Scheduler = scheduler;
        }

        public Accessor Channel { get; set; }

        public IRoutingRule ToRoutingRule()
        {
            return new ScheduledJobRoutingRule<T>();
        }

        public void RegisterJob(IJobTimer timer, IJobExecutor executor, JobStatus status)
        {
            throw new NotImplementedException();
        }

        public JobStatus ToNewJobStatus(JobExecutionRecord record, DateTimeOffset now)
        {
            throw new NotImplementedException();
        }

        public Type JobType
        {
            get { return typeof (T); }
        }

        public IScheduleRule Scheduler { get; private set; }

        public void Reschedule(DateTimeOffset now, JobSchedule schedule)
        {
            var status = schedule.Find(JobType);
            var next = Scheduler.ScheduleNextTime(now);

            if (next != status.NextTime)
            {
                schedule.Schedule(JobType, next);
            }
        }
    }
}