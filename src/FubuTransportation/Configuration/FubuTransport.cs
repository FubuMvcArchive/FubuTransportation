using System;
using FubuMVC.Core;
using FubuCore;
using FubuMVC.Core.Registration.DSL;

namespace FubuTransportation.Configuration
{
    /// <summary>
    /// Use to bootstrap a FubuTransportation application that is not co-hosted with a FubuMVC
    /// application
    /// </summary>
    public static class FubuTransport
    {
        public static IContainerFacilityExpression For<T>() where T : FubuTransportRegistry, new()
        {
            var extension = new T();

            return For(extension);
        }

        public static IContainerFacilityExpression For(FubuTransportRegistry extension)
        {
            var registry = new FubuRegistry();
            extension.As<IFubuRegistryExtension>().Configure(registry);
            return FubuApplication.For(registry);
        }

        public static IContainerFacilityExpression For(Action<FubuTransportRegistry> configuration)
        {
            var extension = FubuTransportRegistry.For(configuration);
            return For(extension);
 
        }

        static FubuTransport()
        {
            Reset();
        }

        public static void Reset()
        {
            UseSynchronousLogging = ApplyMessageHistoryWatching = AllQueuesInMemory = false;
        }

        public static bool UseSynchronousLogging { get; set; }
        public static bool ApplyMessageHistoryWatching { get; set; }
        public static bool AllQueuesInMemory { get; set; }

        public static void SetupForInMemoryTesting()
        {
            UseSynchronousLogging = ApplyMessageHistoryWatching = AllQueuesInMemory = true;
        }
    }
}
