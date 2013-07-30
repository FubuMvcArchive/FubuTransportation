using System;

namespace FubuTransportation.Runtime.Routing
{
    public interface IRoutingRule
    {
        bool Matches(Type type);
    }
}