using System;
using System.Diagnostics;
using FubuCore.Logging;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Runtime;
using FubuTransportation.Configuration;
using FubuTransportation.Logging;

namespace FubuTransportation.Runtime
{
    // TODO -- need to apply unit tests to this thing as the error handling req's
    // solidify
    public class MessageInvoker : IMessageInvoker
    {
        private readonly IServiceFactory _factory;
        private readonly HandlerGraph _graph;
        private readonly IEnvelopeSerializer _serializer;
        private readonly ILogger _logger;

        public MessageInvoker(IServiceFactory factory, HandlerGraph graph, IEnvelopeSerializer serializer, ILogger logger)
        {
            _factory = factory;
            _graph = graph;
            _serializer = serializer;
            _logger = logger;
        }

        public IEnvelopeSerializer Serializer
        {
            get { return _serializer; }
        }

        // TODO -- clean this up. Think it can get simpler
        public IOutgoingMessages Invoke(Envelope envelope, IMessageCallback callback)
        {
            if (envelope.Message == null)
            {
                _serializer.Deserialize(envelope);
            }

            var inputType = envelope.Message.GetType();

            // TODO -- going to get rid of this in favor of a formal "Batch" concept
            if (inputType == typeof (object[]))
            {
                var chain = _graph.ChainFor(typeof (object[]));
                return executeChain(envelope, chain, callback);
            }
            else
            {
                var chain = _graph.ChainFor(inputType);
                if (chain == null)
                {
                    // TODO -- got to do something here for error handling or broadcasting
                    throw new NotImplementedException();
                }

                return executeChain(envelope, chain, callback);
            }
        }

        private IOutgoingMessages executeChain(Envelope envelope, HandlerChain chain, IMessageCallback callback)
        {
            _logger.InfoMessage(() => new ChainExecutionStarted
            {
                ChainId = chain.UniqueId,
                Envelope = envelope
            });

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var args = new HandlerArguments(envelope);
            var behavior = _factory.BuildBehavior(args, chain.UniqueId);

            try
            {
                behavior.Invoke();
                callback.MarkSuccessful();

                return args;
            }
            catch (Exception ex)
            {
                // TODO -- um, do something here
                throw;
                //callback.MarkFailed();
            }
            finally
            {
                stopwatch.Stop();

                _logger.InfoMessage(() => new ChainExecutionFinished
                {
                    ChainId = chain.UniqueId,
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                    Envelope = envelope
                });
            }
        }
    }
}