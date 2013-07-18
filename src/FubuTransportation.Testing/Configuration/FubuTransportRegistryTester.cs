using System.Reflection;
using FubuTransportation.Configuration;
using NUnit.Framework;
using FubuTestingSupport;

namespace FubuTransportation.Testing.Configuration
{
    [TestFixture]
    public class FubuTransportRegistryTester
    {
        [Test]
        public void find_the_calling_assembly()
        {
            FubuTransportRegistry.FindTheCallingAssembly()
                                 .ShouldEqual(Assembly.GetExecutingAssembly());
        }
    }
}