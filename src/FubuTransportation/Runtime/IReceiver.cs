using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation.Runtime
{
    public interface IReceiver
    {
        void Receive(Envelope envelope, IMessageCallback callback);
    }
}