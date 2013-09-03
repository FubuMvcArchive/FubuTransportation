using FubuCore.Logging;
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
        private readonly ILogger _logger;
        private readonly Envelope _envelope;

        public MessageExecutor(IPartialFactory factory, IFubuRequest request, HandlerGraph graph, ILogger logger, Envelope envelope)
        {
            _factory = factory;
            _request = request;
            _graph = graph;
            _logger = logger;
            _envelope = envelope;
        }

        public void Execute(object message)
        {
            var inputType = message.GetType();
            _request.Set(inputType, message);

            var chain = _graph.ChainFor(inputType);

            if (chain == null)
            {
                throw new NoHandlerException(inputType);
            }

            _factory.BuildPartial(chain).InvokePartial();
            _logger.DebugMessage(() => new InlineMessageProcessed
            {
                Envelope = _envelope,
                Message = message
            });

            _request.Clear(inputType);
        }
    }

    public class InlineMessageProcessed : LogRecord
    {
        public object Message { get; set; }
        public Envelope Envelope { get; set; }
    }
}