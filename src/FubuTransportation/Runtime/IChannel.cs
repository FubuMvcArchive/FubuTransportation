using System;
using FubuTransportation.Runtime.Headers;

namespace FubuTransportation.Runtime
{
    public interface IChannel
    {
        Uri Address { get; }
        void Receive(IReceiver receiver);
        void Send(byte[] data, IHeaders headers);
    }
}