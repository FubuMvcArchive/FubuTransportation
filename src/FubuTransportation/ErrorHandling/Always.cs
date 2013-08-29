using System;
using FubuTransportation.Runtime;

namespace FubuTransportation.ErrorHandling
{
    public class Always : IExceptionMatch
    {
        public bool Matches(Envelope envelope, Exception ex)
        {
            return true;
        }
    }
}