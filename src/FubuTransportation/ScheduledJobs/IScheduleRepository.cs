using System;

namespace FubuTransportation.ScheduledJobs
{
    public interface IScheduleRepository
    {
        void Persist(Action<JobSchedule> scheduling);
        void Persist(JobStatus status);
    }
}