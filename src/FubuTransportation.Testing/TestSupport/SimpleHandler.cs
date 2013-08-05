namespace FubuTransportation.Testing.TestSupport
{
    public class SimpleHandler<T> where T : Message
    {
        public void Handle(T message)
        {
            TestMessageRecorder.Processed(GetType().Name, message);
        }
    }
}