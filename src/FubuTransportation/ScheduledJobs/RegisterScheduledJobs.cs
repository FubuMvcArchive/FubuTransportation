using System.Collections.Generic;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.ObjectGraph;

namespace FubuTransportation.ScheduledJobs
{
    public class RegisterScheduledJobs : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            var scheduledJobs = graph.Settings.Get<ScheduledJobGraph>();
            scheduledJobs.Jobs.Each(x => {
                graph.Services.SetServiceIfNone(typeof(IScheduledJob<>).MakeGenericType(x.JobType), ObjectDef.ForValue(x));
            });
        }
    }
}