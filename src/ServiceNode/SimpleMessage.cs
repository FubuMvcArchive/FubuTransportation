using System.Reflection;

namespace ServiceNode
{
    public class SimpleMessage
    {
         
    }

    public class ErrorMessage
    {
        
    }

    public abstract class SimpleHandler<T>
    {
        public void Handle(T message)
        {
            // do nothing for now
        }
    }

    public class SimpleMessageHandler : SimpleHandler<SimpleMessage>
    {
        
    }

    public class ErrorMessageHandler
    {
        public void Handle(ErrorMessage message)
        {
            throw new AmbiguousMatchException();
        }
    }
}