using System;
using System.Collections.Generic;
using System.Linq;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation.ErrorHandling
{
    public class ErrorHandler : IErrorHandler, IErrorCondition
    {
        private readonly IList<IErrorCondition> _conditions = new List<IErrorCondition>(); 

        public IContinuation Continuation = new MoveToErrorQueue();

        public void AddCondition(IErrorCondition condition)
        {
            _conditions.Add(condition);
        }

        public IEnumerable<IErrorCondition> Conditions
        {
            get { return _conditions; }
        }

        public IContinuation DetermineContinuation(Envelope envelope, Exception ex)
        {
            return Matches(envelope, ex) ? Continuation : null;
        }

        public bool Matches(Envelope envelope, Exception ex)
        {
            if (!_conditions.Any()) return true;

            return _conditions.All(x => x.Matches(envelope, ex));
        }
    }
}