using System;
using System.Threading.Tasks;
using FubuCore.Reflection;
using FubuTransportation.Polling;
using FubuTransportation.Runtime.Routing;

namespace FubuTransportation.ScheduledJobs
{
    public interface IScheduledJob
    {
        Type JobType { get; }
        IScheduleRule Scheduler { get; }
        void Initialize(IJobExecutor executor, JobSchedule schedule);
        Accessor Channel { get; }

        IRoutingRule ToRoutingRule();

    }


    public class ScheduledJob<T> : IScheduledJob where T : IJob
    {
        public ScheduledJob(IScheduleRule scheduler)
        {
            Scheduler = scheduler;
        }

        public Accessor Channel { get; set; }

        public IRoutingRule ToRoutingRule()
        {
            return new ScheduledJobRoutingRule<T>();
        }

        public Type JobType
        {
            get { return typeof (T); }
        }

        public IScheduleRule Scheduler { get; private set; }
        public JobExecutionRecord LastExecution { get; set; }

        public void Initialize(IJobExecutor executor, JobSchedule schedule)
        {
            var status = schedule.Find(JobType);
            LastExecution = status.LastExecution;

            var next = Scheduler.ScheduleNextTime(executor.Now());

            schedule.Schedule(JobType, next);

            executor.Schedule<T>(this, next);

        }
    }
}