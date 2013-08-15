using System;
using System.Diagnostics;
using FubuCore.Logging;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Runtime;
using FubuTransportation.Configuration;
using FubuTransportation.Logging;
using System.Collections.Generic;
using FubuCore;

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
        private readonly IEnvelopeSender _sender;

        public MessageInvoker(IServiceFactory factory, HandlerGraph graph, IEnvelopeSerializer serializer, ILogger logger, IEnvelopeSender sender)
        {
            _factory = factory;
            _graph = graph;
            _serializer = serializer;
            _logger = logger;
            _sender = sender;
        }

        public IEnvelopeSerializer Serializer
        {
            get { return _serializer; }
        }

        // TODO -- clean this up. Think it can get simpler
        public void Invoke(Envelope envelope, IMessageCallback callback)
        {
            if (envelope.Message == null)
            {
                _serializer.Deserialize(envelope);
            }

            _logger.InfoMessage(() => new EnvelopeReceived{Envelope = envelope});

            // Do nothing for responses other than kick out the EnvelopeReceived.
            if (envelope.ResponseId.IsNotEmpty())
            {
                _logger.InfoMessage(() => new MessageSuccessful { Envelope = envelope });
                return;
            }

            var inputType = envelope.Message.GetType();

            // TODO -- going to get rid of this in favor of a formal "Batch" concept
            if (inputType == typeof (object[]))
            {
                var chain = _graph.ChainFor(typeof (object[]));
                executeChain(envelope, chain, callback);
            }
            else
            {
                var chain = _graph.ChainFor(inputType);
                if (chain == null)
                {
                    _logger.InfoMessage(() => new NoHandlerForMessage{Envelope = envelope});
                    return;
                }

                executeChain(envelope, chain, callback);
            }
        }

        private void executeChain(Envelope envelope, HandlerChain chain, IMessageCallback callback)
        {
            _logger.DebugMessage(() => new ChainExecutionStarted
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
                args.Each(o =>
                {
                    var child = envelope.ForResponse(o);
                    _sender.Send(child);
                });

                callback.MarkSuccessful();
                _logger.InfoMessage(() => new MessageSuccessful{Envelope = envelope});
            }
            catch (Exception ex)
            {
                // TODO -- um, do something here
                callback.MarkFailed();
                _logger.InfoMessage(() => new MessageFailed{Envelope = envelope, Exception = ex});
                _logger.Error(envelope.CorrelationId, ex);                
            }
            finally
            {
                stopwatch.Stop();

                _logger.DebugMessage(() => new ChainExecutionFinished
                {
                    ChainId = chain.UniqueId,
                    ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                    Envelope = envelope
                });
            }
        }
    }
}