using System;
using System.Threading.Tasks;
using FubuCore.Dates;
using FubuTransportation.Runtime;
using System.Linq;
using System.Collections.Generic;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation
{
    public interface IServiceBus
    {
        Task<TResponse> Request<TRequest, TResponse>(TRequest request);

        void Send<T>(T message);

        /// <summary>
        /// Invoke consumers for the relevant messages managed by the current
        /// service bus instance. This happens immediately and on the current thread.
        /// Error actions will not be executed and the message consumers will not be retried
        /// if an error happens.
        /// </summary>
        void Consume<T>(T message);

        void DelaySend<T>(T message, DateTime time);
        void DelaySend<T>(T message, TimeSpan delay);
    }

    public class ServiceBus : IServiceBus
    {
        private readonly IEnvelopeSender _sender;
        private readonly IEventAggregator _events;
        private readonly IMessageInvoker _invoker;
        private readonly ISystemTime _systemTime;

        public ServiceBus(IEnvelopeSender sender, IEventAggregator events, IMessageInvoker invoker, ISystemTime systemTime)
        {
            _sender = sender;
            _events = events;
            _invoker = invoker;
            _systemTime = systemTime;
        }

        public Task<TResponse> Request<TRequest, TResponse>(TRequest request)
        {
            var envelope = new Envelope
            {
                Message = request,
                ReplyRequested = true
            };

            var listener = new ReplyListener<TResponse>(_events, envelope.CorrelationId);
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
    }
}