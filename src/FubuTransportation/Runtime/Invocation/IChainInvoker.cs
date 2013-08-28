using FubuTransportation.Configuration;

namespace FubuTransportation.Runtime.Invocation
{
    public interface IChainInvoker
    {
        void Invoke(Envelope envelope);
        void InvokeNow<T>(T message);
        void ExecuteChain(Envelope envelope, HandlerChain chain);
        HandlerChain FindChain(Envelope envelope);
    }
}