using System;
using FubuCore;
using FubuTransportation.Polling;
using FubuTransportation.Runtime.Routing;

namespace FubuTransportation.ScheduledJobs
{
    public class ScheduledJobRoutingRule<T> : IRoutingRule where T : IJob
    {
        public bool Matches(Type type)
        {
            return type == typeof (ExecuteScheduledJob<T>);
        }

        public string Describe()
        {
            return "Executes Scheduled job: " + typeof (T).GetFullName();
        }
    }
}