using System.Collections.Generic;

namespace FubuTransportation.ScheduledJobs
{
    public static class JobStatusExtensions
    {
        public static JobSchedule ToSchedule(this IEnumerable<JobStatus> statuses)
        {
            return new JobSchedule(statuses);
        }
    }
}