namespace FubuTransportation.Runtime
{
    public interface IMessageInvoker
    {
        IOutgoingMessages Invoke(Envelope envelope, IMessageCallback callback);
    }
}