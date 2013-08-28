using System.Linq;
using FubuCore.Logging;

namespace FubuTransportation.Runtime.Invocation
{
    public interface IContinuation
    {
        void Execute(Envelope envelope, ILogger logger);
    }

    // Another handler for no subscriber rules!
    public interface IEnvelopeHandler
    {
        IContinuation Handle(Envelope envelope);
    }


    public abstract class SimpleEnvelopeHandler : IEnvelopeHandler, IContinuation
    {
        public IContinuation Handle(Envelope envelope)
        {
            return Matches(envelope) ? this : null;
        }

        public abstract bool Matches(Envelope envelope);

        public abstract void Execute(Envelope envelope, ILogger logger);
    }
}