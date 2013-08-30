using System;
using FubuMVC.Core.Registration;

namespace FubuTransportation.Configuration
{
    public class PoliciesExpression
    {
        private readonly Action<IConfigurationAction> _registration;

        public PoliciesExpression(Action<IConfigurationAction> registration)
        {
            _registration = registration;
        }

        /// <summary>
        ///  Registers a new Policy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public PoliciesExpression Policy<T>() where T : IConfigurationAction, new()
        {
            _registration(new T());
            return this;
        }
    }
}