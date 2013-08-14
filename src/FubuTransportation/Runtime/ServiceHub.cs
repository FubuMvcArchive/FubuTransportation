namespace FubuTransportation.Runtime
{
    public class ServiceHub
    {
        private readonly IEnvelopeSender _sender;
        private readonly IMessageInvoker _invoker;

        public ServiceHub(IEnvelopeSender sender, IMessageInvoker invoker)
        {
            _sender = sender;
            _invoker = invoker;
        }

        public IEnvelopeSender Sender
        {
            get { return _sender; }
        }

        public IMessageInvoker Invoker
        {
            get { return _invoker; }
        }
    }
}