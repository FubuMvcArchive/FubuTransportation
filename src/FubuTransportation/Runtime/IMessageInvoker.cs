namespace FubuTransportation.Runtime
{
    public interface IMessageInvoker
    {
        void Invoke(Envelope envelope);
    }
}