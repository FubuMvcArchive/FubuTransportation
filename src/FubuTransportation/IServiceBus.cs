using System;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;

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
}