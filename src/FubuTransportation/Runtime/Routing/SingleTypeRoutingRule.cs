using System;

namespace FubuTransportation.Runtime.Routing
{
    public class SingleTypeRoutingRule<T> : IRoutingRule
    {
        public bool Matches(Type type)
        {
            return type == typeof (T);
        }
    }
}