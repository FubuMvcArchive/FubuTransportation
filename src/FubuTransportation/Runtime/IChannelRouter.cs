using System.Collections.Generic;
using FubuTransportation.Configuration;

namespace FubuTransportation.Runtime
{
    public interface IChannelRouter
    {
        IEnumerable<ChannelNode> FindChannels(Envelope envelope);
    }
}