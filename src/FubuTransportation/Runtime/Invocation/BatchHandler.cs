using System.Collections.Generic;

namespace FubuTransportation.Runtime.Invocation
{
    public class BatchHandler
    {
        private readonly IMessageExecutor _executor;

        public BatchHandler(IMessageExecutor executor)
        {
            _executor = executor;
        }

        public void Handle(object[] messages)
        {
            messages.Each(o => _executor.Execute(o));
        }
    }
}