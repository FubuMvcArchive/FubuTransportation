using System;
using System.Diagnostics;
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

        public ScheduledJobRunner(T job, ILogger logger, ISystemTime systemTime)
        {
            _job = job;
            _logger = logger;
            _systemTime = systemTime;
        }

        public JobExecutionRecord Execute(ExecuteScheduledJob<T> request)
        {
            var record = new JobExecutionRecord();

            _logger.InfoMessage(() => new ScheduledJobStarted(_job));
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                _job.Execute();

                record.Success = true;
                _logger.InfoMessage(() => new ScheduledJobSucceeded(_job));
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

                _logger.InfoMessage(() => new ScheduledJobFinished(_job, duration));
            }

            return record;
        }
    }
}