using System;
using FubuCore;

namespace FubuTransportation.Runtime
{
    public interface IChannel : IDisposable
    {
        Uri Id { get; }
        void StartReceiving(ChannelOptions options, IReceiver receiver);
    }

    public class ChannelOptions
    {
        public int ThreadCount = 1;
    }

    public interface ITransport : IDisposable, IChannel
    {
        // Really for identification

        // Envelope might have a reference to its parent
        //void Send(Uri destination, Envelope envelope);

        bool Matches(Uri uri);
    }
}