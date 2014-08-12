using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Util;
using FubuTransportation.Configuration;

namespace FubuTransportation.ScheduledJobs
{
    // TODO -- register this thing
    // Going to be tested strictly through integration tests
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

        public void Persist(JobStatus status)
        {
            _persistence.Persist(status.ToDTO(_channels.Name));
        }
    }


}