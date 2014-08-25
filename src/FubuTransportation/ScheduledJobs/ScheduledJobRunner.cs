using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FubuCore;
using FubuCore.Dates;
using FubuCore.Logging;
using FubuTransportation.Polling;

namespace FubuTransportation.ScheduledJobs
{
    public class ScheduledJobRunner<T> where T : IJob
    {
        private readonly T _job;
        private readonly ILogger _logger;
        private readonly ISystemTime _systemTime;
        private readonly IScheduleRepository _repository;

        public ScheduledJobRunner(T job, ILogger logger, ISystemTime systemTime, IScheduleRepository repository)
        {
            _job = job;
            _logger = logger;
            _systemTime = systemTime;
            _repository = repository;
        }


        public JobExecutionRecord Execute(ExecuteScheduledJob<T> request)
        {
            var record = new JobExecutionRecord();

            _logger.InfoMessage(() => new ScheduledJobStarted(_job));
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                _repository.MarkExecuting<T>();

                var timeout = new JobTimeout(request.Timeout);
                timeout.ExecuteSynchronously(_job);


                record.Success = true;
                _logger.InfoMessage(() => new ScheduledJobSucceeded(_job));
            }
            catch (AggregateException ex)
            {
                _logger.Error("Scheduled job {0} failed".ToFormat(_job), ex);
                _logger.InfoMessage(() => new ScheduledJobFailed(_job, ex));
                record.Success = false;
                record.ExceptionText = ex.InnerExceptions.Select(x => x.ToString()).Join("\n");

                // TODO -- might throw this later
            }
            catch (Exception ex)
            {
                _logger.Error("Scheduled job {0} failed".ToFormat(_job), ex);
                _logger.InfoMessage(() => new ScheduledJobFailed(_job, ex));
                record.Success = false;
                record.ExceptionText = ex.ToString();

                // TODO -- might throw this later
            }
            finally
            {
                stopwatch.Stop();
                var duration = stopwatch.ElapsedMilliseconds;
                record.Finished = _systemTime.UtcNow();

                record.Duration = duration;

                _repository.MarkCompletion<T>(record);
                _logger.InfoMessage(() => new ScheduledJobFinished(_job, duration));
            }

            return record;
        }
    }
}