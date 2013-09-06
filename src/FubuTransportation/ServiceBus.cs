using System;
using System.Threading.Tasks;
using FubuCore;
using FubuCore.Dates;
using FubuTransportation.Events;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation
{
    public class ServiceBus : IServiceBus
    {
        private readonly IEnvelopeSender _sender;
        private readonly IEventAggregator _events;
        private readonly IChainInvoker _invoker;
        private readonly ISystemTime _systemTime;

        public ServiceBus(IEnvelopeSender sender, IEventAggregator events, IChainInvoker invoker, ISystemTime systemTime)
        {
            _sender = sender;
            _events = events;
            _invoker = invoker;
            _systemTime = systemTime;
        }

        public Task<TResponse> Request<TResponse>(object request, TimeSpan? timeout = null)
        {
            timeout = timeout ?? 10.Minutes();

            var envelope = new Envelope
            {
                Message = request,
                ReplyRequested = true
            };

            var listener = new ReplyListener<TResponse>(_events, envelope.CorrelationId, timeout.Value);
            _events.AddListener(listener);

            _sender.Send(envelope);

            return listener.Task;
        }

        public void Send<T>(T message)
        {
            _sender.Send(new Envelope {Message = message});
        }

        public void Consume<T>(T message)
        {
            _invoker.InvokeNow(message);
        }

        public void DelaySend<T>(T message, DateTime time)
        {
            _sender.Send(new Envelope
            {
                Message = message,
                ExecutionTime = time.ToUniversalTime()
            });
        }

        public void DelaySend<T>(T message, TimeSpan delay)
        {
            DelaySend(message, _systemTime.UtcNow().Add(delay));
        }

        public Task SendAndWait<T>(T message)
        {
            var envelope = new Envelope
            {
                Message = message,
                AckRequested = true
            };

            var listener = new ReplyListener<Acknowledgement>(_events, envelope.CorrelationId, 10.Minutes());
            _events.AddListener(listener);

            _sender.Send(envelope);

            return listener.Task;
        }
    }
}