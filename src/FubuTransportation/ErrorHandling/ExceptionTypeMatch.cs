using System;
using FubuTransportation.Runtime;

namespace FubuTransportation.ErrorHandling
{
    public class ExceptionTypeMatch<T> : IExceptionMatch where T : Exception
    {
        public bool Matches(Envelope envelope, Exception ex)
        {
            return ex is T;
        }
    }
}