using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FubuTransportation.Monitoring
{
    public class HealthAndAssignmentRouter : IDisposable
    {
        private readonly IPersistentTasks _tasks;
        private readonly IEnumerable<ITransportPeer> _peers;

        public HealthAndAssignmentRouter(IPersistentTasks tasks, IEnumerable<ITransportPeer> peers)
        {
            _tasks = tasks;
            _peers = peers;
        }

        public Task EnsureAllTasksAreAssignedAndRunning()
        {
            var assigned = _peers.SelectMany(x => x.CurrentlyOwnedSubjects()).ToArray();
            var assignments = _tasks.PersistentSubjects
                .Where(x => !assigned.Contains(x))
                .Select(StartAssignment).ToArray();


            var healthTasks =_peers.Select(
                    peer => peer.CheckStatusOfOwnedTasks()
                        .ContinueWith(t => t.Result.Tasks.Each(ReassignIfNecessary)));

            return Task.Factory.ContinueWhenAll(healthTasks.Union(assignments).ToArray(), _ => { });
        }

        public void ReassignIfNecessary(PersistentTaskStatus status)
        {
            if (status.Status == HealthStatus.Active) return;

            // TODO -- log

            Reassign(status.Subject);
        }

        public Task Reassign(Uri subject)
        {
            // log here
            var agent = _tasks.FindAgent(subject);
            if (agent == null)
            {
                // TODO -- do something here. Log obviously

                return Task.Factory.StartNew(() => { });
            }

            var existing = _peers.FirstOrDefault(x => x.CurrentlyOwnedSubjects().Contains(subject));
            if (existing != null)
            {
                existing.Deactivate(subject);
            }

            return StartAssignment(subject);
        }

        public Task StartAssignment(Uri subject)
        {
            // TODO --log here

            var agent = _tasks.FindAgent(subject);
            if (agent == null)
            {
                // TODO -- do something here. Log obviously

                return Task.Factory.StartNew(() => { });
            }
            
            var existing = _peers.FirstOrDefault(x => x.CurrentlyOwnedSubjects().Contains(subject));
            if (existing != null)
            {
                existing.Deactivate(subject);
            }


            // TODO -- assumes that the agent itself activates
            // the task

            // TODO --log  stuff
            return agent.AssignOwner(_peers);
        }

        public void Dispose()
        {
        }
    }
}