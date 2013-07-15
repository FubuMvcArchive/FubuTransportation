using System.Threading.Tasks;

namespace FubuTransportation
{
    public interface IServiceBus
    {
        Task<TResponse> Request<TRequest, TResponse>(TRequest request);

        // MUST be a receiver or it fails
        void Send(params object[] messages);

        // Doesn't have to be a receiver
        void Publish(params object[] messages);
        void ConsumeMessages(params object[] messages);
    }

    public interface IEventAggregator
    {
        
    }
}