using System;
using System.Collections.Generic;
using FubuCore;
using FubuTransportation.Configuration;

namespace FubuTransportation.Runtime
{
    public interface ITransport : IDisposable
    {
        void OpenChannels(ChannelGraph graph);
    }
}