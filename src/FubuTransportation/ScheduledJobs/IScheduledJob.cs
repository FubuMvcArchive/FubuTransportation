using System;
using FubuCore.Reflection;
using FubuTransportation.Polling;
using FubuTransportation.Runtime.Routing;

namespace FubuTransportation.ScheduledJobs
{
    public interface IScheduledJob
    {
        Type JobType { get; }
        void Initialize(IJobExecutor executor, JobSchedule schedule);
        Accessor Channel { get; }
        TimeSpan Timeout { get;}
        IRoutingRule ToRoutingRule();
    }

    public interface IScheduledJob<T> where T : IJob
    {
        void Execute(IJobExecutor executor);
    }
}