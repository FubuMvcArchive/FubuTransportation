using FubuTestingSupport;
using FubuTransportation.Polling;
using FubuTransportation.Runtime;
using FubuTransportation.Testing.Events;
using FubuTransportation.Testing.Polling;
using NUnit.Framework;

namespace FubuTransportation.Testing.Runtime
{
    [TestFixture]
    public class EnvelopeTokenTester
    {
        [Test]
        public void is_not_related_to_a_job_with_no_message_at_all()
        {
            new EnvelopeToken().IsPollingJobRelated()
                .ShouldBeFalse();
        }

        [Test]
        public void is_not_related_to_a_job_with_a_normal_message()
        {
            new EnvelopeToken{Message = new Message1()}
                .IsPollingJobRelated()
                .ShouldBeFalse();
        }

        [Test]
        public void is_related_to_a_job_if_the_message_is_polling_job_request()
        {
            new EnvelopeToken
            {
                Message   = new JobRequest<APollingJob>()
            }.IsPollingJobRelated()
            .ShouldBeTrue();
        }
    }
}