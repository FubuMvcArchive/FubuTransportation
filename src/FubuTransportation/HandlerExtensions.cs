using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuMVC.Core.Registration;
using FubuTransportation.Configuration;
using FubuTransportation.Registration.Nodes;

namespace FubuTransportation
{
    public static class HandlerExtensions
    {
        [MarkedForTermination]
        public static bool MatchesSagaConvention(this Type type)
        {
            return type.Name.EndsWith("Saga") && type.HasProperty("State") && type.HasProperty("IsCompleted");
        }

        public static IEnumerable<HandlerCall> Handlers(this BehaviorGraph graph)
        {
            var handlerGraph = graph.Settings.Get<HandlerGraph>();
            return handlerGraph.SelectMany(behavior => behavior.OfType<HandlerCall>().Distinct());
        }
    }
}