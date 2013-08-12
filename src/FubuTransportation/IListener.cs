namespace FubuTransportation
{
    public interface IListener<T>
    {
        void Handle(T message);
    }
}