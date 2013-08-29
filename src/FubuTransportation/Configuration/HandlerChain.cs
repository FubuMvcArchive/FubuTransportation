using System;
using System.Collections.Generic;
using FubuMVC.Core.Registration.Nodes;
using FubuTransportation.ErrorHandling;
using FubuTransportation.Registration.Nodes;

namespace FubuTransportation.Configuration
{
    public class HandlerChain : BehaviorChain, IMayHaveInputType
    {
        public static readonly string Category = "Handler";
        public readonly IList<IErrorHandler> ErrorHandlers = new List<IErrorHandler>();
        public int MaximumAttempts = 1;

        public HandlerChain()
        {
            UrlCategory.Category = Category;
            IsPartialOnly = true;
        }

        public HandlerChain(IEnumerable<HandlerCall> calls) : this()
        {
            calls.Each(AddToEnd);
        }

        
    }
}