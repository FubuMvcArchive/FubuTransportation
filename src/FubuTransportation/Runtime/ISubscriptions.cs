using System.Collections.Generic;
using FubuTransportation.Configuration;

namespace FubuTransportation.Runtime
{
    public interface ISubscriptions
    {
        IEnumerable<ChannelNode> FindChannels(Envelope envelope);
        void Start();

        ChannelNode ReplyNodeFor(ChannelNode destination);
    }
}