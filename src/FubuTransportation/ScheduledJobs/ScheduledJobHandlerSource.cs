using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuTransportation.Registration;
using FubuTransportation.Registration.Nodes;

namespace FubuTransportation.ScheduledJobs
{
    public class ScheduledJobHandlerSource : IHandlerSource
    {
        public readonly IList<Type> JobTypes = new List<Type>(); 

        public IEnumerable<HandlerCall> FindCalls()
        {
            return JobTypes.Select<Type, HandlerCall>(type => {
                return typeof (ScheduledJobHandlerCall<>).CloseAndBuildAs<HandlerCall>(type);

            });
        }
    }
}