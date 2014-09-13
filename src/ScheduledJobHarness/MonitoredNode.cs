using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.ElasticLoadBalancing.Model;
using FubuCore;
using FubuCore.Logging;
using FubuCore.Util;
using FubuMVC.Core;
using FubuMVC.StructureMap;
using FubuTransportation;
using FubuTransportation.Configuration;
using FubuTransportation.Monitoring;
using FubuTransportation.Polling;
using FubuTransportation.RavenDb;
using FubuTransportation.ScheduledJobs;
using FubuTransportation.ScheduledJobs.Persistence;
using FubuTransportation.Subscriptions;

namespace ScheduledJobHarness
{
    public class ErrorHandlingPolicy : HandlerChainPolicy
    {
        public override void Configure(HandlerChain handlerChain)
        {
            //when retrying, do so a maximum of 5 times
            handlerChain.MaximumAttempts = 3;

            //retry now
            handlerChain.OnException<DivideByZeroException>()
                .RetryLater(5.Seconds());
        }
    }

    public class MonitoredNode : FubuTransportRegistry<MonitoringSettings>, IDisposable
    {
        public static readonly Random Random = new Random(100);

        public const string HealthyAndFunctional = "Healthy and Functional";
        public const string TimesOutOnStartupOrHealthCheck = "Times out on startup or health check";
        public const string ThrowsExceptionOnStartupOrHealthCheck = "Throws exception on startup or health check";
        public const string IsInactive = "Is inactive";

        private readonly string _nodeId;
        private FubuRuntime _runtime;

        public MonitoredNode(string nodeId, Uri incoming)
        {
            Local.Policy<ErrorHandlingPolicy>();

            AlterSettings<MonitoringSettings>(x => x.Incoming = incoming);
            NodeName = "Monitoring";
            NodeId = nodeId;

            _nodeId = nodeId;

            EnableInMemoryTransport(incoming);

            Services(_ => {
                _.ReplaceService<ISubscriptionPersistence, RavenDbSubscriptionPersistence>();
                _.ReplaceService<ISchedulePersistence, RavenDbSchedulePersistence>();

                _.AddService<ILogListener, ScheduledJobListener>();
            });


            ScheduledJob.DefaultJobChannel(x => x.Incoming);
            ScheduledJob.RunJob<AlwaysGoodJob>().ScheduledBy<FiveMinuteIncrements>();
            ScheduledJob.RunJob<FailsFirstTwoTimes>().ScheduledBy<ThreeMinuteIncrements>();
            ScheduledJob.RunJob<FailsSometimes>().ScheduledBy<SevenMinuteIncrements>();
            ScheduledJob.RunJob<TimesOutSometimes>().ScheduledBy<TenMinuteIncrements>();

        }

        public string Id
        {
            get { return _nodeId; }
        }

        public ScheduledJobGraph Jobs
        {
            get
            {
                return _runtime.Factory.Get<ScheduledJobGraph>();
            }
        }

        public void Startup(ISubscriptionPersistence subscriptions, ISchedulePersistence schedules)
        {
            Services(_ => {
                _.ReplaceService(subscriptions);
                _.ReplaceService(schedules);
            });

            _runtime = FubuTransport.For(this).StructureMap().Bootstrap();
        }

        void IDisposable.Dispose()
        {
            _runtime.Dispose();
        }

        public void Shutdown()
        {
            _runtime.Dispose();
        }
    }
}