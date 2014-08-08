using System;
using FubuCore;
using FubuTransportation.Registration.Nodes;

namespace FubuTransportation.ScheduledJobs
{
    public class ScheduledJob
    {
        // TODO -- need to add channel here.

        public ScheduledJob(Type jobType, IScheduleRule scheduler)
        {
            JobType = jobType;
            Scheduler = scheduler;
        }

        public Type JobType { get; private set; }
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

        public HandlerCall ToHandlerCall()
        {
            return typeof (ScheduledJobHandlerCall<>)
                .CloseAndBuildAs<HandlerCall>(JobType);
        }
    }
}