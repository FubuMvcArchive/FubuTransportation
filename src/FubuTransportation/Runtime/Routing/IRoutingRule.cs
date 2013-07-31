using System;

namespace FubuTransportation.Runtime.Routing
{
    public interface IRoutingRule
    {
        bool Matches(Type type);
    }

    public class SingleTypeRoutingRule<T> : IRoutingRule
    {
        public bool Matches(Type type)
        {
            return type == typeof (T);
        }
    }
}