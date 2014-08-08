using System;
using FubuCore;
using FubuCore.Logging;

namespace FubuTransportation.ScheduledJob
{
    public class ScheduledJobStarted : LogRecord
    {
        public string Description { get; set; }

        public override string ToString()
        {
            return "Scheduled job {0} started".ToFormat(Description);
        }
    }

    public class ScheduledJobSucceeded : LogRecord
    {
        public string Description { get; set; }

        public override string ToString()
        {
            return "Scheduled job {0} succeeded".ToFormat(Description);
        }
    }

    public class ScheduledJobFailed : LogRecord
    {
        public string Description { get; set; }
        public Exception Exception { get; set; }

        public override string ToString()
        {
            return "Scheduled job {0} failed with exception {1}".ToFormat(Description, Exception);
        }
    }

    public class ScheduledJobFinished : LogRecord
    {
        public string Description { get; set; }
        public long Duration { get; set; }

        public override string ToString()
        {
            return "Scheduled job {0} finished in {1} ms".ToFormat(Description, Duration);
        }
    }
}