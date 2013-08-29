using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Headers;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation.Testing
{
    public class RecordingReceiver : IReceiver
    {
        public IList<Envelope> Received = new List<Envelope>(); 

        public void Receive(byte[] data, IHeaders headers, IMessageCallback callback)
        {
            var envelope = new Envelope(data, headers, callback);
            Received.Add(envelope);

            envelope.Callback.MarkSuccessful();
        }
    }
}