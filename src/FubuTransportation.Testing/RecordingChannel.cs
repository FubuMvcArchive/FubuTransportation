using System;
using System.Collections.Generic;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Headers;

namespace FubuTransportation.Testing
{
    public class RecordingChannel : IChannel
    {
        public readonly IList<Envelope> Sent = new List<Envelope>();

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Uri Address { get; private set; }
        public void StartReceiving(IReceiver receiver, ChannelNode node)
        {
            throw new NotImplementedException();
        }

        public void Send(byte[] data, IHeaders headers)
        {
            var envelope = new Envelope(headers) {Data = data};
            Sent.Add(envelope);
        }
    }
}