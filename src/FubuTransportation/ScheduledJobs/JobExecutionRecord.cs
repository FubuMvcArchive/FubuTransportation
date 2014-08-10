using System;

namespace FubuTransportation.ScheduledJobs
{
    public class JobExecutionRecord
    {
        public TimeSpan Duration { get; set; }
        public DateTimeOffset Finished { get; set; }
        public bool Success { get; set; }
        public string ExceptionText { get; set; }
    }
}