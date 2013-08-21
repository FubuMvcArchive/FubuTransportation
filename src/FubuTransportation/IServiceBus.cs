using System;
using System.Threading.Tasks;
using FubuTransportation.Runtime;
using System.Linq;
using System.Collections.Generic;

namespace FubuTransportation
{
    public interface IServiceBus
    {
        Task<TResponse> Request<TRequest, TResponse>(TRequest request);

        // MUST be a receiver or it fails
        void Send<T>(T message);

        // Doesn't have to be a receiver
        //void Publish(params object[] messages);


        /// <summary>
        /// Invoke consumers for the relevant messages managed by the current
        /// service bus instance. This happens immediately and on the current thread.
        /// Error actions will not be executed and the message consumers will not be retried
        /// if an error happens.
        /// </summary>
        void Consume<T>(T message);


    }

    public class ServiceBus : IServiceBus
    {
        private readonly IEnvelopeSender _sender;
        private readonly IEventAggregator _events;

        public ServiceBus(IEnvelopeSender sender, IEventAggregator events)
        {
            _sender = sender;
            _events = events;
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
            throw new NotImplementedException();
        }
    }
}