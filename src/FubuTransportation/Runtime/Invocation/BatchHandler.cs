using FubuMVC.Core.Runtime;
using System.Collections.Generic;
using FubuTransportation.Configuration;

namespace FubuTransportation.Runtime.Invocation
{
    public class BatchHandler
    {
        private readonly IPartialFactory _factory;
        private readonly IFubuRequest _request;
        private readonly HandlerGraph _graph;

        public BatchHandler(IPartialFactory factory, IFubuRequest request, HandlerGraph graph)
        {
            _factory = factory;
            _request = request;
            _graph = graph;
        }

        public void Handle(object[] messages)
        {
            messages.Each(o => {
                var inputType = o.GetType();
                _request.Set(inputType, o);

                var chain = _graph.ChainFor(inputType);

                // TODO -- do something when the chain is not found - THROW

                _factory.BuildPartial(chain).InvokePartial();

                _request.Clear(inputType);
            });
        }
    }
}