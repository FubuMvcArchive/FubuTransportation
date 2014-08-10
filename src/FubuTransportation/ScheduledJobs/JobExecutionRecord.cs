using System;

namespace FubuTransportation.ScheduledJobs
{
    public class JobExecutionRecord
    {
        public double Duration { get; set; }
        public DateTimeOffset Finished { get; set; }
        public bool Success { get; set; }
        public string ExceptionText { get; set; }

        protected bool Equals(JobExecutionRecord other)
        {
            return Duration.Equals(other.Duration) && Finished.Equals(other.Finished) && Success.Equals(other.Success);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((JobExecutionRecord) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Duration.GetHashCode();
                hashCode = (hashCode*397) ^ Finished.GetHashCode();
                hashCode = (hashCode*397) ^ Success.GetHashCode();
                return hashCode;
            }
        }
    }
}