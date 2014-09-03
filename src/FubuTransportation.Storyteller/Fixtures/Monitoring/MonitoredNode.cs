using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using FubuCore;
using FubuCore.Util;
using FubuMVC.Core;
using FubuMVC.Diagnostics.Model;
using FubuMVC.StructureMap;
using FubuTransportation.Configuration;
using FubuTransportation.Monitoring;
using FubuTransportation.Polling;
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

        private readonly IList<Uri> _initialTasks = new List<Uri>();
        private readonly Cache<string, FakePersistentTaskSource> _sources = new Cache<string, FakePersistentTaskSource>(scheme => new FakePersistentTaskSource(scheme)); 
            
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
            HealthMonitoring
                .ScheduledExecution(monitoringEnabled ? ScheduledExecution.WaitUntilInterval : ScheduledExecution.Disabled)
                .IntervalSeed(3);

            return Task.Factory.StartNew(() => FubuTransport.For(this).StructureMap().Bootstrap()).ContinueWith(t => {
                _runtime = t.Result;

                var controller = _runtime.Factory.Get<IPersistentTaskController>();
                _initialTasks.Each(subject => {
                    controller.TakeOwnership(subject).ContinueWith(t1 => {
                        if (t1.Result != OwnershipStatus.OwnershipActivated)
                        {
                            throw new Exception("Unable to activate {0} on node {1}".ToFormat(subject, Id));
                        }
                    });

                });

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

        public Task ActivateTask(Uri subject)
        {
            return _runtime.Factory.Get<IPersistentTaskController>().TakeOwnership(subject);
        }

        public void AddInitialTask(Uri subject)
        {
            _initialTasks.Add(subject);
        }

        public IEnumerable<TaskState> AssignedTasks()
        {
            var controller = _runtime.Factory.Get<IPersistentTaskController>();
            return controller.ActiveTasks().Select(uri => new TaskState {Node = Id, Task = uri});
        }

        public Task WaitForHealthCheck()
        {
            var jobs = _runtime.Factory.Get<IPollingJobs>();
            if (jobs.IsActive<HealthMonitorPollingJob>())
            {
                return jobs.WaitForJobToExecute<HealthMonitorPollingJob>();
            }
            else
            {
                return jobs.ExecuteJob<HealthMonitorPollingJob>();
            }
        }

        public void Shutdown()
        {
            _runtime.Dispose();
        }
    }
}