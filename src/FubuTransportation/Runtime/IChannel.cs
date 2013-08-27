using System;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime.Headers;

namespace FubuTransportation.Runtime
{
    public interface IChannel : IDisposable
    {
        Uri Address { get; }
        void StartReceiving(IReceiver receiver, ChannelNode node);

        void Send(byte[] data, IHeaders headers);
    }
}