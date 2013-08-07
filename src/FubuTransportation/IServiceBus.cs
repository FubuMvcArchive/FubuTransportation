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
        void Send(params object[] messages);

        // Doesn't have to be a receiver
        //void Publish(params object[] messages);
        
        void ConsumeMessages(params object[] messages);
    }

    public class ServiceBus : IServiceBus
    {
        private readonly IChannelRouter _router;
        private readonly IEnvelopeSerializer _serializer;

        public ServiceBus(IChannelRouter router, IEnvelopeSerializer serializer)
        {
            _router = router;
            _serializer = serializer;
        }

        public Task<TResponse> Request<TRequest, TResponse>(TRequest request)
        {
            return new TaskCompletionSource<TResponse>().Task;
        }

        public void Send(params object[] messages)
        {
            if (messages.Count() == 1)
            {
                var envelope = new Envelope(null)
                {
                    Message = messages.Single()
                };

                _serializer.Serialize(envelope);

                var channels = _router.FindChannels(envelope.Message).ToArray();
                if (!channels.Any())
                {
                    throw new Exception("No channels match this message");
                }

                channels.Each(x => x.Send(envelope));
            }
            else
            {
                throw new NotImplementedException("Not yet batching");
            }
        }

        public void ConsumeMessages(params object[] messages)
        {
        }
    }

    public interface IEventAggregator
    {
        
    }
}