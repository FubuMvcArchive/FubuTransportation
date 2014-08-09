using System;
using FubuCore;
using FubuTransportation.Polling;
using FubuTransportation.Registration.Nodes;

namespace FubuTransportation.ScheduledJobs
{
    public interface IScheduledJob
    {
        Type JobType { get; }
        IScheduleRule Scheduler { get; }
        void Reschedule(DateTimeOffset now, JobSchedule schedule);
    }

    public class ScheduledJob<T> : IScheduledJob where T : IJob
    {
        // TODO -- need to add channel here.

        public ScheduledJob(IScheduleRule scheduler)
        {
            Scheduler = scheduler;
        }

        public Type JobType
        {
            get
            {
                return typeof (T);
            }
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