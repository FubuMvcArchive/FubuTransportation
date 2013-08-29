using System;
using FubuTransportation.ErrorHandling;
using FubuTransportation.Runtime;
using FubuTransportation.Testing.Runtime;
using NUnit.Framework;
using FubuTestingSupport;

namespace FubuTransportation.Testing.ErrorHandling
{
    [TestFixture]
    public class when_creating_an_error_report_from_the_exception_and_envelope
    {
        Envelope envelope = ObjectMother.Envelope();
        Exception exception  = new NotImplementedException();
        private ErrorReport report;

        [SetUp]
        public void SetUp()
        {
            envelope.Message = new Message1();
            report = new ErrorReport(envelope, exception);
            
        }

        [Test]
        public void capture_the_exception_message()
        {
            report.ExceptionMessage.ShouldEqual(exception.Message);
        }

        [Test]
        public void capture_the_headers()
        {
            report.Headers.ShouldEqual(envelope.Headers);
        }

        [Test]
        public void explanation_is_via_an_exception()
        {
            report.Explanation.ShouldEqual(ErrorReport.ExceptionDetected);
        }

        [Test]
        public void exception_stack_trace_is_captured()
        {
            report.ExceptionText.ShouldEqual(exception.ToString());
        }

        [Test]
        public void exception_type_is_captured()
        {
            report.ExceptionType.ShouldEqual(exception.GetType().FullName);
        }


    }
}