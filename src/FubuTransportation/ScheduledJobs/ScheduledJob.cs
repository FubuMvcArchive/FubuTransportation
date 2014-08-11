using System;
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
        void Reschedule(JobExecutionRecord record, IJobExecutor executor);

        Accessor Channel { get; }

        IRoutingRule ToRoutingRule();
    }

    public interface IScheduledJob<T> : IScheduledJob
    {
       
    }


    public class ScheduledJob<T> : IScheduledJob<T> where T : IJob
    {
        public ScheduledJob(IScheduleRule scheduler)
        {
            Scheduler = scheduler;
        }

        public void Reschedule(JobExecutionRecord record, IJobExecutor executor)
        {
            if (record.Success)
            {
                LastExecution = record;
                var next = Scheduler.ScheduleNextTime(executor.Now());

                executor.ResetExecution<T>(this, next, record);
            }
            else
            {
                throw new NotImplementedException();
            }
            
            
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