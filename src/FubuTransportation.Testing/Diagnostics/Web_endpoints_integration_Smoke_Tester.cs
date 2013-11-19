using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuMVC.Katana;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Diagnostics.Visualization;
using NUnit.Framework;

namespace FubuTransportation.Testing.Diagnostics
{
    [TestFixture]
    public class Web_endpoints_integration_Smoke_Tester
    {
        [Test]
        public void the_message_handlers_visualization_can_be_shown()
        {
            using (var server = EmbeddedFubuMvcServer.For<DiagnosticApplication>())
            {
                server.Endpoints.Get<MessagesFubuDiagnostics>(x => x.get_messages())
                    .StatusCode.ShouldEqual(HttpStatusCode.OK);
            }
        }
    }

    public class DiagnosticApplication : FubuTransportRegistry, IApplicationSource
    {
        public DiagnosticApplication()
        {
            EnableInMemoryTransport();
        }

        public FubuApplication BuildApplication()
        {
            var registry = new FubuRegistry();
            registry.Import<DiagnosticApplication>();

            return FubuApplication.For(registry).StructureMap();
        }
    }
}