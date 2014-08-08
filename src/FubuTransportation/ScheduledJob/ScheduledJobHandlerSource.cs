using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FubuTransportation.Polling;
using FubuTransportation.Registration;
using FubuTransportation.Registration.Nodes;

namespace FubuTransportation.ScheduledJob
{
    public class ScheduledJobHandlerSource : IHandlerSource
    {
        private readonly IList<Type> _jobTypes = new List<Type>();
 
        public void AddJobType(Type type)
        {
            _jobTypes.Add(type);
        }

        public bool HasAny()
        {
            return _jobTypes.Any();
        }

        public IEnumerable<HandlerCall> FindCalls()
        {
            var executeMethodName = GetExecuteMethodName();

            return _jobTypes.Select(x => {
                var handlerType = typeof (ScheduledJobRunner<>).MakeGenericType(x);
                var method = handlerType.GetMethod(executeMethodName);

                return new HandlerCall(handlerType, method);
            });
        }

        private static string GetExecuteMethodName()
        {
            Expression<Action<ScheduledJobRunner<IJob>>> expression = x => x.Execute(null);
            var memberExpression = (MethodCallExpression)expression.Body;
            return memberExpression.Method.Name;
        }
    }
}