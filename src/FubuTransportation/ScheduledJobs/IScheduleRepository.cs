using System;

namespace FubuTransportation.ScheduledJobs
{
    public interface IScheduleRepository
    {
        void Persist(Action<JobSchedule> scheduling);

        void MarkScheduled<T>(DateTimeOffset nextTime);
        void MarkExecuting<T>();
        void MarkCompletion<T>(JobExecutionRecord record);
    }
}