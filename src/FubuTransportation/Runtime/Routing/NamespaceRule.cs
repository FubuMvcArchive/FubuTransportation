using System;
using FubuCore;

namespace FubuTransportation.Runtime.Routing
{
    public class NamespaceRule : IRoutingRule
    {
        private readonly string _ns;

        public NamespaceRule(string @namespace)
        {
            _ns = @namespace;
        }

        public bool Matches(Type type)
        {
            return type.IsInNamespace(_ns);
        }

        public static NamespaceRule For<T>()
        {
            return new NamespaceRule(typeof(T).Namespace);
        }
    }
}