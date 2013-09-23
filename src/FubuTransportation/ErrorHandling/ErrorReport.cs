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

        protected bool Equals(ErrorReport other)
        {
            return Equals(Message, other.Message) && string.Equals(ExceptionText, other.ExceptionText);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ErrorReport) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Message != null ? Message.GetHashCode() : 0)*397) ^ (ExceptionText != null ? ExceptionText.GetHashCode() : 0);
            }
        }
    }
}