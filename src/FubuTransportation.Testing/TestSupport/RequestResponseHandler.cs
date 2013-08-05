namespace FubuTransportation.Testing.TestSupport
{
    public class RequestResponseHandler<T> where T : Message
    {
        public MirrorMessage<T> Handle(T message)
        {
            TestMessageRecorder.Processed(GetType().Name, message);
            return new MirrorMessage<T> {Id = message.Id};
        }
    }
}