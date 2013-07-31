using System.Collections.Generic;
using System.Linq;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;

namespace FubuTransportation.InMemory
{
    public class InMemoryTransport : ITransport
    {
        public void Dispose()
        {
            // nothing
        }

        public void OpenChannels(ChannelGraph graph)
        {
            graph.Where(x => x.Protocol() == InMemoryChannel.Protocol).Each(x => x.Channel = new InMemoryChannel(x));
        }
    }
}