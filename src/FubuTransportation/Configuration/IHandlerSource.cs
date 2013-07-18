using System.Collections.Generic;

namespace FubuTransportation.Configuration
{
    public interface IHandlerSource
    {
        IEnumerable<HandlerCall> FindCalls();
    }
}