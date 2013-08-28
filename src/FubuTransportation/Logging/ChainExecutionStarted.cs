using System;
using FubuCore.Logging;
using FubuTransportation.Runtime;

namespace FubuTransportation.Logging
{
    public class ChainExecutionStarted : LogRecord
    {
        // TODO -- add a decent ToString()
        public Guid ChainId { get; set; }
        public EnvelopeToken Envelope { get; set; }
    }
}