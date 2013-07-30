using System;
using System.Reflection;

namespace FubuTransportation.Runtime.Routing
{
    public class AssemblyRule : IRoutingRule
    {
        private readonly Assembly _assembly;

        public AssemblyRule(Assembly assembly)
        {
            _assembly = assembly;
        }

        public bool Matches(Type type)
        {
            return _assembly.Equals(type.Assembly);
        }

        public static AssemblyRule For<T>()
        {
            return new AssemblyRule(typeof(T).Assembly);
        }
    }
}