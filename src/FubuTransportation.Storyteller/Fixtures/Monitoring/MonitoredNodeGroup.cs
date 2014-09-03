using System;
using System.Collections.Generic;
using System.Threading;
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
        private readonly Cache<Uri, string> _initialAssignments = new Cache<Uri, string>(); 

        public void Add(string nodeId, Uri incoming)
        {
            var node = new MonitoredNode(nodeId, incoming);
            _nodes[nodeId] = node;
        }

        public void AddTask(Uri subject, string initialNode, IEnumerable<string> preferredNodes)
        {
            _configurations.Add(node => node.AddTask(subject, preferredNodes));

            if (!initialNode.EqualsIgnoreCase("none"))
            {
                _initialAssignments[subject] = initialNode;
            }

        }

        public bool MonitoringEnabled { get; set; }

        public Task Startup()
        {
            // startup each node
            // remember to do the initial assignments
            throw new NotImplementedException();      
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
            throw new NotImplementedException();
        }

        public IEnumerable<TaskState> PersistedTasks()
        {
            throw new NotImplementedException();
        } 
    }

    public class TaskState
    {
        public Uri Task { get; set; }
        public string Node { get; set; }
    }
}