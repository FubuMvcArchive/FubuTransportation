namespace FubuTransportation.Runtime
{
    public interface IMessageInvoker
    {
        void Invoke(Envelope envelope, IMessageCallback callback);
        void InvokeNow<T>(T message);
    }
}