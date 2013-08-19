using FubuTransportation.Configuration;
using NUnit.Framework;
using FubuTestingSupport;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class FubuTransport_default_options_Tester
    {
        [Test]
        public void synchronous_event_aggregation_is_defaulted_to_false()
        {
            FubuTransport.UseSynchronousLogging = true;
            FubuTransport.Reset();

            FubuTransport.UseSynchronousLogging.ShouldBeFalse();
        }

        [Test]
        public void message_watching_is_defaulted_to_false()
        {
            FubuTransport.ApplyMessageHistoryWatching = true;
            FubuTransport.Reset();

            FubuTransport.ApplyMessageHistoryWatching.ShouldBeFalse();
        }

    }
}