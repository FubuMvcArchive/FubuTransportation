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
        public Uri Subject { get; set; }
    }

    public class TakeOwnershipResponse
    {
        public Uri Subject { get; set; }
        public OwnershipStatus Status { get; set; }
        public string NodeId { get; set; }
    }

    public enum OwnershipStatus
    {
        Activated,
        Exception,
        AlreadyOwned,
        UnknownSubject
    }

}