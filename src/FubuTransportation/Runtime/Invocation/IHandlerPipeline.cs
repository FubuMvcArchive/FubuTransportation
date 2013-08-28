namespace FubuTransportation.Runtime.Invocation
{
    public interface IHandlerPipeline
    {
        void Invoke(Envelope envelope);
    }
}