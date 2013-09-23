using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using FubuCore.Descriptions;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation.ErrorHandling
{
    public class ErrorHandler : IErrorHandler, IExceptionMatch, DescribesItself
    {
        private readonly IList<IExceptionMatch> _conditions = new List<IExceptionMatch>(); 

        public IContinuation Continuation = new RequeueContinuation();

        public void AddCondition(IExceptionMatch condition)
        {
            _conditions.Add(condition);
        }

        public IEnumerable<IExceptionMatch> Conditions
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

        public void Describe(Description description)
        {
            description.Title = "Error Handler";
            if (_conditions.Count == 1)
            {
                description.AddChild("Condition", _conditions.Single());
            }
            else if (_conditions.Count > 1)
            {
                description.AddList("Conditions", _conditions);
            }

            description.AddChild("Continuation", Continuation);
        }
    }
}