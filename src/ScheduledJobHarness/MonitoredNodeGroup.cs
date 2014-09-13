using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using FubuCore;
using FubuCore.Util;
using FubuMVC.Core;
using FubuMVC.Katana;
using FubuMVC.OwinHost;
using FubuMVC.StructureMap;
using FubuTransportation.Configuration;
using FubuTransportation.ScheduledJobs;
using FubuTransportation.ScheduledJobs.Persistence;
using FubuTransportation.Subscriptions;
using StructureMap;

namespace ScheduledJobHarness
{
    public class MonitoredNodeGroup : FubuRegistry,IDisposable
    {
        private readonly Cache<string, MonitoredNode> _nodes = new Cache<string, MonitoredNode>();

        // TODO -- replace w/ RavenDb later
        private readonly ISubscriptionPersistence _subscriptions = new InMemorySubscriptionPersistence();
        private readonly ISchedulePersistence _schedules = new InMemorySchedulePersistence();
        private readonly int _port;
        private FubuRuntime _runtime;
        private readonly ManualResetEvent _reset = new ManualResetEvent(false);

        public MonitoredNodeGroup()
        {
            _port = PortFinder.FindPort(5500);

            AlterSettings<KatanaSettings>(_ => {
                _.AutoHostingEnabled = true;
                _.Port = _port;
            });

            Services(_ => {
                _.ReplaceService(_subscriptions);
                _.ReplaceService(_schedules);

            });

            Import<MonitoredTransportRegistry>();
        }

        public class MonitoredTransportRegistry : FubuTransportRegistry
        {
            public MonitoredTransportRegistry()
            {
                EnableInMemoryTransport();
            }
        }

        public IEnumerable<MonitoredNode> Nodes()
        {
            return _nodes;
        }

        public void Shutdown()
        {
            _reset.Set();
        }

        public int Port
        {
            get { return _port; }
        }

        public void Add(string nodeId, Uri incoming)
        {
            var node = new MonitoredNode(nodeId, incoming);
            _nodes[nodeId] = node;
        }

        public MonitoredNode NodeFor(string id)
        {
            return _nodes[id];
        }

        public void Startup()
        {
            _nodes.Each(node => node.Startup(_subscriptions, _schedules));
            
            var container = new Container(_ => {
                _.ForSingletonOf<MonitoredNodeGroup>().Use(this);
            });

            _runtime = FubuApplication.For(this).StructureMap(container).Bootstrap();

            _runtime.Factory.Get<ChannelGraph>().Name = "Monitoring";

            var jobs = _nodes.First.Jobs;
            container.Configure(_ => _.For<ScheduledJobGraph>().Use(jobs));
        }

        public void Dispose()
        {
            _nodes.Each(x => x.SafeDispose());
            _runtime.Dispose();
        }

        public void ShutdownNode(string node)
        {
            _nodes[node].Shutdown();
            _nodes.Remove(node);
        }

        public void WaitForShutdown()
        {
            _reset.WaitOne();
        }
    }

    public class TaskState
    {
        public Uri Task { get; set; }
        public string Node { get; set; }
    }
}