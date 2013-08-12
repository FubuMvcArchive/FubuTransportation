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
        //void ConsumeMessages(params object[] messages);
    }

    public class ServiceBus : IServiceBus
    {
        private readonly IEnvelopeSender _sender;

        public ServiceBus(IEnvelopeSender sender)
        {
            _sender = sender;
        }

        public Task<TResponse> Request<TRequest, TResponse>(TRequest request)
        {
            return new TaskCompletionSource<TResponse>().Task;
        }

        public void Send<T>(T message)
        {
            _sender.Send(new Envelope {Message = message});
        }
    }
}