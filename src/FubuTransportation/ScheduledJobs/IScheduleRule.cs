using System;

namespace FubuTransportation.ScheduledJobs
{
    public interface IScheduleRule
    {
        DateTimeOffset ScheduleNextTime(DateTimeOffset currentTime);
    }
}