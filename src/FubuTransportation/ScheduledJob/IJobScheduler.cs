using System;

namespace FubuTransportation.ScheduledJob
{
    public interface IJobScheduler
    {
        DateTimeOffset ScheduleNextTime(DateTimeOffset currentTime);
    }
}