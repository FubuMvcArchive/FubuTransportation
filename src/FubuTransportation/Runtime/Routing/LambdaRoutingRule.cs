using System;

namespace FubuTransportation.Runtime.Routing
{
    public class LambdaRoutingRule : IRoutingRule
    {
        private readonly Func<Type, bool> _filter;

        public LambdaRoutingRule(Func<Type, bool> filter)
        {
            _filter = filter;
        }

        public bool Matches(Type type)
        {
            return _filter(type);
        }
    }
}