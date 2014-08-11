using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Reflection;
using FubuMVC.Core.Registration;

namespace FubuTransportation.ScheduledJobs
{
    [ApplicationLevel]
    public class ScheduledJobGraph
    {
        public readonly IList<IScheduledJob> Jobs = new List<IScheduledJob>();
        public Accessor DefaultChannel { get; set; }

        public void DetermineSchedule(IJobExecutor executor, JobSchedule schedule)
        {
            // Make sure that all existing jobs are schedules
            Jobs.Each(x => x.Initialize(executor, schedule));

            var types = Jobs.Select(x => x.JobType).ToArray();
            schedule.RemoveObsoleteJobs(types);
        }

        public IScheduledJob FindJob(Type jobType)
        {
            return Jobs.FirstOrDefault(x => x.JobType == jobType);
        }
    }
}