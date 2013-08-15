using NUnit.Framework;
using FubuTestingSupport;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class TransportSettingsTester
    {
        [Test]
        public void debug_is_disabled_by_default()
        {
            new TransportSettings().DebugEnabled.ShouldBeFalse();
        }

    }
}