using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FubuMVC.Core;
using FubuTransportation.Configuration;
using FubuTransportation.Events;
using NUnit.Framework;
using StructureMap;
using FubuMVC.StructureMap;
using FubuTestingSupport;

namespace FubuTransportation.Testing.Configuration
{
    [TestFixture]
    public class AllInMemoryQueues_Mechanics_Tester
    {
        private Container container;
        private FubuRuntime runtime;

        [SetUp]
        public void SetUp()
        {
            FubuTransport.AllQueuesInMemory = true;

            container = new Container();
            var registry = new FubuRegistry();
            registry.Import<AllInMemoryRegistry>();
            registry.Import<AnotherRegistry>();

            runtime = FubuApplication.For(registry).StructureMap(container).Bootstrap();

        }

        [TearDown]
        public void TearDown()
        {

            

            var container = runtime.Factory.Get<IContainer>();
            Debug.WriteLine(container.Model.PluginTypes.Count());

            var types = container.Model.PluginTypes.ToArray();
            for (int i = 0; i < 6; i++)
            {
                var configuration = types[i];
                Debug.WriteLine("Flushing " + configuration.PluginType.FullName);
                configuration.EjectAndRemoveAll();
            }

//            container.Model.PluginTypes.Where(x => x.Lifecycle == "Singleton")
//                .Each(x => {
//                    Debug.WriteLine(x.PluginType.FullName);
//                    x.EjectAndRemoveAll();
//                });

            runtime.Dispose();
        }

        [Test]
        public void we_remember_all_of_the_setting_types()
        {
            var transportSettings = container.GetInstance<TransportSettings>();
            transportSettings.SettingTypes.ShouldContain(typeof(BusSettings));
            transportSettings.SettingTypes.ShouldContain(typeof(AnotherSettings));
        }

        [Test]
        public void if_in_memory_queues_for_all_derive_the_queue_uri_and_have_it_injected_into_container()
        {

            var busSettings = container.GetInstance<BusSettings>();
            busSettings.Downstream.ToString().ShouldEqual("memory://bus/downstream");

            var anotherSettings = container.GetInstance<AnotherSettings>();
            anotherSettings.Destination.ToString().ShouldEqual("memory://another/destination");

        }
    }

    public class AllInMemoryRegistry : FubuTransportRegistry<BusSettings>
    {
        
    }

    public class AnotherRegistry : FubuTransportRegistry<AnotherSettings>
    {
        
    }

    public class AnotherSettings
    {
        public Uri Destination { get; set; }
    }
}