using System;
using System.Net;
using FubuMVC.Core;
using FubuMVC.Katana;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.LightningQueues.Diagnostics;
using NUnit.Framework;

namespace FubuTransportation.LightningQueues.Testing
{
    public class Diagnostic_endpoints_integration_Smoke_Tester
    {
        [Test]
        public void the_lightning_queues_summary_page_can_be_shown()
        {
            using (var server = EmbeddedFubuMvcServer.For<LightningQueuesDiagnosticsApplication>())
            {
                server.Endpoints.Get<LightningQueuesFubuDiagnostics>(x => x.Index())
                    .StatusCode.ShouldEqual(HttpStatusCode.OK);
            }
        } 
    }

    public class LightningQueuesDiagnosticsApplication : IApplicationSource
    {
        public FubuApplication BuildApplication()
        {
            var registry = new FubuRegistry();
            registry.Import<LightningQueuesDiagnosticsTransportRegistry>();

            return FubuApplication.For(registry).StructureMap();
        }
    }

    public class LightningQueuesDiagnosticsTransportRegistry : FubuTransportRegistry<TransportDiagnosticsSettings>
    {
        public LightningQueuesDiagnosticsTransportRegistry()
        {
            Channel(x => x.Endpoint)
                .ReadIncoming();
        }
    }

    public class TransportDiagnosticsSettings
    {
        public TransportDiagnosticsSettings()
        {
            Endpoint = new Uri("lq.tcp://localhost:2020/diagnostics");
        }

        public Uri Endpoint { get; set; }
    }
}