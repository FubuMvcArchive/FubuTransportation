namespace FubuTransportation.Runtime.Invocation
{
    public interface IMessageCallback
    {
        void MarkSuccessful();
        void MarkFailed();

        void MoveToDelayed();
    }
}