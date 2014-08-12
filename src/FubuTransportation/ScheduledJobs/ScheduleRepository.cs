using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Util;
using FubuTransportation.Configuration;

namespace FubuTransportation.ScheduledJobs
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly ChannelGraph _channels;
        private readonly ISchedulePersistence _persistence;
        private readonly Cache<string, Type> _jobTypes = new Cache<string, Type>(x => null);

        public ScheduleRepository(ChannelGraph channels, ScheduledJobGraph jobs, ISchedulePersistence persistence)
        {
            _channels = channels;
            _persistence = persistence;

            jobs.Jobs.Each(j => {
                var key = JobStatus.GetKey(j.JobType);
                _jobTypes[key] = j.JobType;
            });
        }


        public void Persist(Action<JobSchedule> scheduling)
        {
            var schedule = _persistence.FindAll(_channels.Name).Select(ToStatus)
                .Where(x => x.JobType != null).ToSchedule();

            scheduling(schedule);

            _persistence.Persist(schedule.Select(x => x.ToDTO(_channels.Name)));
        }

        public JobStatus ToStatus(JobStatusDTO dto)
        {
            return new JobStatus(_jobTypes[dto.JobKey])
            {
                Status = dto.Status,
                LastExecution = dto.LastExecution,
                NextTime = dto.NextTime
            };
        }

        private void modifyStatus<T>(Action<JobStatusDTO> change)
        {
            var jobKey = JobStatus.GetKey(typeof (T));
            var status = _persistence.Find(_channels.Name, jobKey);

            change(status);

            _persistence.Persist(status);
        }

        public void MarkScheduled<T>(DateTimeOffset nextTime)
        {
            modifyStatus<T>(_ => {
                _.Status = JobExecutionStatus.Scheduled;
                _.NextTime = nextTime;
            });
        }

        public void MarkExecuting<T>()
        {
            modifyStatus<T>(_ => { _.Status = JobExecutionStatus.Executing; });
        }

        public void MarkCompletion<T>(JobExecutionRecord record)
        {
            modifyStatus<T>(_ => {
                _.Status = JobExecutionStatus.Completed;
                _.LastExecution = record;
            });
        }
    }
}