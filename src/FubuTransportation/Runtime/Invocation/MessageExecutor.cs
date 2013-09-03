using FubuMVC.Core.Runtime;
using FubuTransportation.Configuration;

namespace FubuTransportation.Runtime.Invocation
{
    // Tested only through integration tests
    public class MessageExecutor : IMessageExecutor
    {
        private readonly IPartialFactory _factory;
        private readonly IFubuRequest _request;
        private readonly HandlerGraph _graph;

        public MessageExecutor(IPartialFactory factory, IFubuRequest request, HandlerGraph graph)
        {
            _factory = factory;
            _request = request;
            _graph = graph;
        }

        public void Execute(object message)
        {
            var inputType = message.GetType();
            _request.Set(inputType, message);

            var chain = _graph.ChainFor(inputType);

            // TODO -- do something when the chain is not found - THROW

            _factory.BuildPartial(chain).InvokePartial();

            _request.Clear(inputType);
        }
    }
}