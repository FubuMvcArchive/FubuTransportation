using System;
using FubuCore.Logging;
using FubuTransportation.Runtime;

namespace FubuTransportation.Logging
{
    public class ChainExecutionFinished : LogRecord
    {
        // TODO -- add a decent ToString()

        public Guid ChainId { get; set; }
        public EnvelopeToken Envelope { get; set; }
        public long ElapsedMilliseconds { get; set; }
    }
}