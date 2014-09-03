using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FubuCore;
using FubuCore.Util;
using FubuTransportation.Subscriptions;

namespace FubuTransportation.Storyteller.Fixtures.Monitoring
{
    public class MonitoredNodeGroup : IDisposable
    {
        private readonly Cache<string, MonitoredNode> _nodes = new Cache<string, MonitoredNode>();
        private readonly InMemorySubscriptionPersistence _persistence = new InMemorySubscriptionPersistence();
        private readonly IList<Action<MonitoredNode>> _configurations = new List<Action<MonitoredNode>>();

        public void Add(string nodeId, Uri incoming)
        {
            var node = new MonitoredNode(nodeId, incoming);
            _nodes[nodeId] = node;
        }

        public void AddTask(Uri subject, string initialNode, IEnumerable<string> preferredNodes)
        {
            _configurations.Add(node => node.AddTask(subject, preferredNodes));

            _nodes[initialNode].AddInitialTask(subject);
        }

        public bool MonitoringEnabled { get; set; }

        public MonitoredNode NodeFor(string id)
        {
            return _nodes[id];
        }

        public Task Startup()
        {
            var tasks = _nodes.Select(node => {
                _configurations.Each(x => x(node));
                return node.Startup(MonitoringEnabled, _persistence);
            });

            return Task.WhenAll(tasks);
        }

        public void Dispose()
        {
            _nodes.Each(x => x.SafeDispose());
        }

        public void SetTaskState(Uri subject, string node, string state)
        {
            var task = _nodes[node].TaskFor(subject);
            task.SetState(state);
        }

        public IEnumerable<TaskState> AssignedTasks()
        {
            return _nodes.SelectMany(x => x.AssignedTasks());
        }

        public IEnumerable<TaskState> PersistedTasks()
        {
            return
                _persistence.AllNodes()
                    .SelectMany(
                        node => { return node.OwnedTasks.Select(x => new TaskState {Node = node.Id, Task = x}); });
        }

        public void WaitForAllHealthChecks()
        {
            var tasks = _nodes.Select(x => x.WaitForHealthCheck());
            Task.WaitAll(tasks.ToArray());
        }

        public void ShutdownNode(string node)
        {
            _nodes[node].Shutdown();
            _nodes.Remove(node);
        }
    }

    public class TaskState
    {
        public Uri Task { get; set; }
        public string Node { get; set; }
    }
}