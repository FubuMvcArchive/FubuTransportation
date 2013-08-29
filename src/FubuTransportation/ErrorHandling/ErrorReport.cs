using System;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Headers;

namespace FubuTransportation.ErrorHandling
{
    public class ErrorReport
    {
        public const string ExceptionDetected = "Exception Detected";
        public object Message { get; set; }

        public ErrorReport(Envelope envelope, Exception ex)
        {
            Message = envelope.Message;
            Headers = envelope.Headers;
            ExceptionText = ex.ToString();
            ExceptionMessage = ex.Message;
            ExceptionType = ex.GetType().FullName;
            Explanation = ExceptionDetected;
        }

        public IHeaders Headers { get; set; }

        public string Explanation { get; set; }

        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionText { get; set; }
    }
}