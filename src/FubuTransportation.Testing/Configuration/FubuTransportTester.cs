using Bottles;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using NUnit.Framework;

namespace FubuTransportation.Testing.Configuration
{
    [TestFixture]
    public class FubuTransportTester
    {
        [SetUp]
        public void SetUp()
        {
            PackageRegistry.Properties.Remove(FubuTransport.FT_TESTING);
            FubuTransport.Reset();
        }

        [Test]
        public void if_package_registry_ft_testing_is_true()
        {
            FubuTransport.Reset();
            
            PackageRegistry.Properties[FubuTransport.FT_TESTING] = "true";

            FubuTransport.InTestingMode().ShouldBeTrue();
        }

        [Test]
        public void if_package_registry_ft_testing_is_missing()
        {
            FubuTransport.Reset();

            PackageRegistry.Properties.Remove(FubuTransport.FT_TESTING);

            FubuTransport.InTestingMode().ShouldBeFalse();
        }

    }
}