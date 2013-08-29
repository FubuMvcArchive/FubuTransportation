using System;
using FubuTransportation.Runtime;

namespace FubuTransportation.ErrorHandling
{
    public class Always : IErrorCondition
    {
        public bool Matches(Envelope envelope, Exception ex)
        {
            return true;
        }
    }
}