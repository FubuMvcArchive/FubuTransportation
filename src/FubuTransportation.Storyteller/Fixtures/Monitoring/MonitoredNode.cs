using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FubuCore.Util;
using FubuMVC.Core;
using FubuMVC.Diagnostics.Model;
using FubuMVC.StructureMap;
using FubuTransportation.Configuration;
using FubuTransportation.Monitoring;
using FubuTransportation.Subscriptions;

namespace FubuTransportation.Storyteller.Fixtures.Monitoring
{
    public class MonitoredNode : FubuTransportRegistry<MonitoringSettings>, IDisposable
    {
        public const string HealthyAndFunctional = "Healthy and Functional";
        public const string TimesOutOnStartupOrHealthCheck = "Times out on startup or health check";
        public const string ThrowsExceptionOnStartupOrHealthCheck = "Throws exception on startup or health check";
        public const string IsInactive = "Is inactive";

        private readonly string _nodeId;
        private FubuRuntime _runtime;

        private readonly Cache<string, FakePersistentTaskSource> _sources = new Cache<string, FakePersistentTaskSource>(scheme => new FakePersistentTaskSource(scheme)); 
            
        // TODO -- need a way to enable/disable polling jobs
        // TODO -- take in Mike's PR first

        public MonitoredNode(string nodeId, Uri incoming)
        {
            AlterSettings<MonitoringSettings>(x => x.Incoming = incoming);
            NodeName = "Monitoring";
            NodeId = nodeId;

            _nodeId = nodeId;


        }

        public FakePersistentTask TaskFor(string uriString)
        {
            return TaskFor(uriString.ToUri());
        }

        public FakePersistentTask TaskFor(Uri uri)
        {
            return _sources[uri.Scheme][uri];
        }

        public string Id
        {
            get { return _nodeId; }
        }

        public Task<FubuRuntime> Startup(bool monitoringEnabled, ISubscriptionPersistence persistence)
        {
            Services(_ => _sources.Each(_.AddService<IPersistentTaskSource>));
            Services(_ => _.ReplaceService(persistence));
            HealthMonitoringEnabled = monitoringEnabled;

            return Task.Factory.StartNew(() => FubuTransport.For(this).StructureMap().Bootstrap()).ContinueWith(t => {
                _runtime = t.Result;
                return t.Result;
            });
        }

        void IDisposable.Dispose()
        {
            _runtime.Dispose();
        }

        public void AddTask(Uri subject, IEnumerable<string> preferredNodes)
        {
            TaskFor(subject).PreferredNodes = preferredNodes;
        }
    }
}