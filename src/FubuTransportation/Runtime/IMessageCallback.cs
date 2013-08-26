namespace FubuTransportation.Runtime
{
    public interface IMessageCallback
    {
        void MarkSuccessful();
        void MarkFailed();

        void MoveToDelayed();
    }
}