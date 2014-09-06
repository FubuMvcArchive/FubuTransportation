using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FubuCore.Logging;
using HtmlTags.Conventions;

namespace FubuTransportation.Monitoring
{
    // Error handling and logging is handled in PersistentTaskController
    public interface IPersistentTaskAgent
    {
        Uri Subject { get; }
        Task<ITransportPeer> AssignOwner(IEnumerable<ITransportPeer> peers);
    }

    public class PersistentTaskAgent : IPersistentTaskAgent
    {
        private readonly IPersistentTask _task;
        private readonly HealthMonitoringSettings _settings;
        private readonly ILogger _logger;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public PersistentTaskAgent(IPersistentTask task, HealthMonitoringSettings settings, ILogger logger)
        {
            _task = task;
            _settings = settings;
            _logger = logger;
        }

        public Uri Subject
        {
            get { return _task.Subject; }
        }

        public Task<HealthStatus> AssertAvailable()
        {
            return Task.Factory.StartNew(() => {
                var status =
                    _lock.Read(
                        () => TimeoutRunner.Run(_settings.TaskAvailabilityCheckTimeout, () => _task.AssertAvailable(),
                            ex => _logger.Error(Subject, "Availability test failed for " + Subject, ex)));

                switch (status)
                {
                    case Completion.Exception:
                        _logger.InfoMessage(() => new TaskAvailabilityFailed(Subject));
                        return HealthStatus.Error;

                    case Completion.Success:
                        return HealthStatus.Active;

                    default:
                        _logger.InfoMessage(() => new TaskAvailabilityFailed(Subject));
                        return HealthStatus.Timedout;
                }
            }, TaskCreationOptions.AttachedToParent);
        }

        public Task Activate()
        {
            return Task.Factory.StartNew(
                () => _lock.Write(() => _task.Activate()),
                TaskCreationOptions.AttachedToParent
                );
        }

        public Task Deactivate()
        {
            return Task.Factory.StartNew(
                () => _lock.Write(() => _task.Deactivate()),
                TaskCreationOptions.AttachedToParent
                );
        }

        public bool IsActive
        {
            get { return _lock.Read(() => _task.IsActive); }
        }

        public Task<ITransportPeer> AssignOwner(IEnumerable<ITransportPeer> peers)
        {
            // TODO -- do some filtering here.
            return _task.SelectOwner(peers);
        }
    }
}