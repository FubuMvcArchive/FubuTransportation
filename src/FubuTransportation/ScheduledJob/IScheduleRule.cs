using System;

namespace FubuTransportation.ScheduledJob
{
    public interface IScheduleRule
    {
        DateTimeOffset ScheduleNextTime(DateTimeOffset currentTime);
    }
}