namespace FubuTransportation.Runtime.Invocation
{
    public interface IMessageInvoker : IEnvelopeHandler
    {
        void Invoke(Envelope envelope);
        void InvokeNow<T>(T message);
    }
}