using System.Diagnostics;

namespace FubuTransportation.Testing.ScenarioSupport
{
    public class RequestResponseHandler<T> where T : Message
    {
        public MirrorMessage<T> Handle(T message)
        {
            TestMessageRecorder.Processed(GetType().Name, message);
            return new MirrorMessage<T> {Id = message.Id};
        }
    }

    public class RequestResponseHandler<TRequest, TResponse> where TRequest : Message where TResponse : Message, new()
    {
        public TResponse Handle(TRequest request)
        {
            Debug.WriteLine("I responded w/ " + typeof(TResponse).Name);

            return new TResponse
            {
                Id = request.Id
            };
        }
    }
}