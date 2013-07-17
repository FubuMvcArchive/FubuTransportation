using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FubuCore;
using FubuCore.Reflection;
using FubuMVC.Core.Registration.Nodes;
using FubuTransportation.Runtime;

namespace FubuTransportation.Configuration
{
    public class HandlerCall : ActionCallBase
    {
        public static HandlerCall For<T>(Expression<Action<T>> method)
        {
            return new HandlerCall(typeof(T), ReflectionHelper.GetMethod(method));
        }

        public HandlerCall(Type handlerType, MethodInfo method)
            : base(handlerType, method)
        {
        }

        public override BehaviorCategory Category
        {
            get { return BehaviorCategory.Call; }
        }

        protected override Type determineHandlerType()
        {
            if (HasOutput && HasInput)
            {
                return typeof (CascadingHandlerInvoker<,,>)
                    .MakeGenericType(
                        HandlerType,
                        Method.GetParameters().First().ParameterType,
                        Method.ReturnType);
            }

            if (!HasOutput && HasInput)
            {
                return typeof (SimpleHandlerInvoker<,>)
                    .MakeGenericType(
                        HandlerType,
                        Method.GetParameters().First().ParameterType);
            }

            throw new FubuException(1005,
                                    "The action '{0}' is invalid. Only methods that support the '1 in 1 out' or '1 in 0 out' patterns are valid as FubuTransportation handlers",
                                    Description);

        }
    }
}