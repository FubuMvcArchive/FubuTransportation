using System;
using FubuTransportation.Configuration;

namespace FubuTransportation.Runtime
{
    public interface IChannel : IDisposable
    {
        Uri Address { get; }
        void StartReceiving(IReceiver receiver, ChannelNode node);

        // TODO -- have some common infrastructure set the address on envelope
        void Send(Envelope envelope);
    }
}