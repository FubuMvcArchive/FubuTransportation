using System;

namespace FubuTransportation.Monitoring
{
    public class TaskHealthRequest
    {
        public Uri[] Subjects { get; set; }
    }

    public class TaskHealthResponse
    {
        public TaskStatus[] Tasks { get; set; }
    }

    public class TaskStatus
    {
        public Uri Subject { get; set; }
        public HealthStatus Status { get; set; }
    }

    public enum HealthStatus
    {
        Active,
        Unknown,
        Error,
        Inactive
    }

    public class TakeOwnershipRequest
    {
        public TakeOwnershipRequest(Uri subject)
        {
            Subject = subject;
        }

        public TakeOwnershipRequest()
        {
        }

        public Uri Subject { get; set; }

        protected bool Equals(TakeOwnershipRequest other)
        {
            return Equals(Subject, other.Subject);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TakeOwnershipRequest) obj);
        }

        public override int GetHashCode()
        {
            return (Subject != null ? Subject.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("TakeOwnershipRequest for subject: {0}", Subject);
        }
    }

    public class TakeOwnershipResponse
    {
        public Uri Subject { get; set; }
        public OwnershipStatus Status { get; set; }
        public string NodeId { get; set; }
    }

    public enum OwnershipStatus
    {
        OwnershipActivated,
        Exception,
        AlreadyOwned,
        UnknownSubject
    }

}