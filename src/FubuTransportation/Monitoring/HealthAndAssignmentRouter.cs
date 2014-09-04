using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FubuCore.Logging;

namespace FubuTransportation.Monitoring
{
    public class HealthAndAssignmentRouter : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IPersistentTasks _tasks;
        private readonly IEnumerable<ITransportPeer> _peers;

        public HealthAndAssignmentRouter(ILogger logger, IPersistentTasks tasks, IEnumerable<ITransportPeer> peers)
        {
            _logger = logger;
            _tasks = tasks;
            _peers = peers;
        }

        public Task EnsureAllTasksAreAssignedAndRunning()
        {
            var assigned = _peers.SelectMany(x => x.CurrentlyOwnedSubjects()).ToArray();
            var assignments = _tasks.PersistentSubjects
                .Where(x => !assigned.Contains(x))
                .Select(StartAssignment).ToArray();

            // TODO -- prolly better make the timeout configurable
            var healthTasks = _peers.Select(
                    peer => peer.CheckStatusOfOwnedTasks()
                        .ContinueWith(t => t.Result.Tasks.Each(x => ReassignIfNecessary(peer, x)))).ToArray();

            return Task.Factory.ContinueWhenAll(healthTasks.Union(assignments).ToArray(), _ => { });
        }

        public void ReassignIfNecessary(ITransportPeer peer, PersistentTaskStatus status)
        {
            if (status.Status == HealthStatus.Active) return;

            _logger.InfoMessage(() => new ReassigningTask(status.Subject, status.Status, peer.NodeId));

            Reassign(status.Subject);
        }

        public Task Reassign(Uri subject)
        {
            // log here
            var agent = _tasks.FindAgent(subject);
            if (agent == null)
            {
                _logger.InfoMessage(() => new UnknownTask(subject, "Trying to reassign a persistent task"));

                return Task.Factory.StartNew(() => { });
            }

            var existing = _peers.FirstOrDefault(x => x.CurrentlyOwnedSubjects().Contains(subject));
            if (existing != null)
            {
                _logger.Debug(() => "Attempting to deactivate persistent task " + subject);
                existing.Deactivate(subject);
                
            }

            return StartAssignment(subject);
        }

        public Task StartAssignment(Uri subject)
        {
            var agent = _tasks.FindAgent(subject);
            if (agent == null)
            {
                _logger.InfoMessage(() => new UnknownTask(subject, "Trying to assign the task owner"));

                return Task.Factory.StartNew(() => { });
            }
            
            var existing = _peers.FirstOrDefault(x => x.CurrentlyOwnedSubjects().Contains(subject));
            if (existing != null)
            {
                existing.Deactivate(subject);
            }

            return agent.AssignOwner(_peers).ContinueWith(t => {
                if (t.IsCompleted && t.Result == null)
                {
                    _logger.InfoMessage(() => new UnableToAssignOwnership(subject));
                }

                if (t.IsFaulted)
                {
                    _logger.Error(subject, "Failed while trying to assign ownership", t.Exception);
                }
            });
        }

        public void Dispose()
        {
        }
    }
}