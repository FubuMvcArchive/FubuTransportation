using System;

namespace FubuTransportation.ScheduledJobs.Execution
{
    public interface IScheduleRule
    {
        DateTimeOffset ScheduleNextTime(DateTimeOffset currentTime);
    }
}