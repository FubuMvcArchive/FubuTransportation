using System;
using Bottles;
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
        public static readonly string FT_TESTING = "FubuTransportTesting";

        private static bool _useSynchronousLogging;
        private static bool _applyMessageHistoryWatching;

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

        public static IContainerFacilityExpression DefaultPolicies()
        {
            return For(x => { });
        }

        static FubuTransport()
        {
            Reset();
        }

        public static void Reset()
        {
            UseSynchronousLogging = ApplyMessageHistoryWatching = AllQueuesInMemory = false;
        }

        public static bool UseSynchronousLogging
        {
            get
            {
                bool returnValue = false;
                if (bool.TryParse(PackageRegistry.Properties[FT_TESTING], out returnValue))
                {
                    return returnValue || _useSynchronousLogging;
                }

                return _useSynchronousLogging;
            }
            set { _useSynchronousLogging = value; }
        }

        public static bool ApplyMessageHistoryWatching
        {
            get
            {
                bool returnValue = false;
                if (bool.TryParse(PackageRegistry.Properties[FT_TESTING], out returnValue))
                {
                    return returnValue || _applyMessageHistoryWatching;
                }
                
                return _applyMessageHistoryWatching;
            }
            set { _applyMessageHistoryWatching = value; }
        }

        public static bool AllQueuesInMemory { get; set; }

        public static void SetupForInMemoryTesting()
        {
            UseSynchronousLogging = ApplyMessageHistoryWatching = AllQueuesInMemory = true;
        }
    }
}
