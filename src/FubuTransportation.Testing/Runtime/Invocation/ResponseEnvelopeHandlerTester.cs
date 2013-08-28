using System;
using FubuMVC.Core.Runtime.Logging;
using FubuTransportation.Runtime.Invocation;
using NUnit.Framework;
using FubuTestingSupport;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Runtime.Invocation
{
    [TestFixture]
    public class ResponseEnvelopeHandlerTester
    {
        [Test]
        public void matches_positive()
        {
            var envelope = ObjectMother.Envelope();
            envelope.ResponseId = Guid.NewGuid().ToString();

            new ResponseEnvelopeHandler().Matches(envelope)
                .ShouldBeTrue();
        }

        [Test]
        public void matches_negative()
        {
            var envelope = ObjectMother.Envelope();
            envelope.ResponseId = null;

            new ResponseEnvelopeHandler().Matches(envelope)
                .ShouldBeFalse();
        }

        [Test]
        public void handle()
        {
            // pretty simple.  Effectively just shuts things down.
            var logger = new RecordingLogger();

                        var envelope = ObjectMother.Envelope();
            envelope.ResponseId = Guid.NewGuid().ToString();

            new ResponseEnvelopeHandler().Execute(envelope, logger);

            envelope.Callback.AssertWasCalled(x => x.MarkSuccessful());
        }
    }
}