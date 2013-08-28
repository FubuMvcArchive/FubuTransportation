using FubuCore.Logging;

namespace FubuTransportation.Runtime.Invocation
{
    public interface IContinuation
    {
        void Execute(Envelope envelope, ILogger logger);
    }
}