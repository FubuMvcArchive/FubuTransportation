using System.Collections;
using System.Collections.Generic;
using FubuCore.Descriptions;

namespace FubuTransportation.Runtime.Invocation
{
    public class CompositeContinuation : IContinuation, IEnumerable<IContinuation>, DescribesItself
    {
        private readonly IList<IContinuation> _continuations = new List<IContinuation>();

        public CompositeContinuation(params IContinuation[] continuations)
        {
            _continuations.AddRange(continuations);
        }

        public void Execute(Envelope envelope, ContinuationContext context)
        {
            _continuations.Each(x => x.Execute(envelope, context));
        }

        public void Add(IContinuation child)
        {
            _continuations.Add(child);
        }


        public IEnumerator<IContinuation> GetEnumerator()
        {
            return _continuations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Describe(Description description)
        {
            description.Title = "Composite Continuation";
            description.AddList("Continuations", _continuations);
        }
    }
}