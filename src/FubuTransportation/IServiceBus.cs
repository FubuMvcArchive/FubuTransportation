using System;
using System.Threading.Tasks;
using FubuCore;
using FubuCore.Dates;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation
{
    public interface IServiceBus
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="timeout">Timespan to wait for a response before timing out</param>
        /// <returns></returns>
        Task<TResponse> Request<TResponse>(object request, TimeSpan? timeout = null);

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
    }
}