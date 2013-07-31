using System;
using FubuTransportation.Configuration;

namespace FubuTransportation.Runtime
{
    public interface IChannel : IDisposable
    {
        Uri Address { get; }
        void StartReceiving(IReceiver receiver, ChannelNode node);
    }
}