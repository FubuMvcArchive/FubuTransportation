using Bottles;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using NUnit.Framework;

namespace FubuTransportation.Testing.Configuration
{
    [TestFixture]
    public class FubuTransportTester
    {
        [Test]
        public void if_package_registry_ft_testing_is_true()
        {
            FubuTransport.Reset();
            
            PackageRegistry.Properties[FubuTransport.FT_TESTING] = "true";

            FubuTransport.ApplyMessageHistoryWatching.ShouldBeTrue();
            FubuTransport.AllQueuesInMemory.ShouldBeFalse(); // Don't want this at all
            FubuTransport.UseSynchronousLogging.ShouldBeTrue();
        }

        [Test]
        public void if_package_registry_ft_testing_is_missing()
        {
            FubuTransport.Reset();

            PackageRegistry.Properties.Remove(FubuTransport.FT_TESTING);

            FubuTransport.ApplyMessageHistoryWatching.ShouldBeFalse();
            FubuTransport.AllQueuesInMemory.ShouldBeFalse(); // Don't want this at all
            FubuTransport.UseSynchronousLogging.ShouldBeFalse();
        }

        [Test]
        public void if_package_registry_ft_testing_is_false()
        {
            FubuTransport.Reset();

            PackageRegistry.Properties[FubuTransport.FT_TESTING] = "false";

            FubuTransport.ApplyMessageHistoryWatching.ShouldBeFalse();
            FubuTransport.AllQueuesInMemory.ShouldBeFalse(); // Don't want this at all
            FubuTransport.UseSynchronousLogging.ShouldBeFalse();
        }
    }
}