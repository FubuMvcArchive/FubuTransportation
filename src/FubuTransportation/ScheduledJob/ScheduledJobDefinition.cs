using System;

namespace FubuTransportation.ScheduledJob
{
    public class ScheduledJobDefinition
    {
        public Type JobType { get; set; }
        public IJobScheduler Scheduler { get; set; }
    }
}