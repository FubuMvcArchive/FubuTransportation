using System.Collections.Generic;
using System.Linq;
using FubuTransportation.Configuration;

namespace FubuTransportation.Runtime
{
    public class ChannelRouter : IChannelRouter
    {
        private readonly ChannelGraph _graph;

        public ChannelRouter(ChannelGraph graph)
        {
            _graph = graph;
        }

        public IEnumerable<ChannelNode> FindChannels(Envelope envelope)
        {
            if (envelope.Destination != null)
            {
                var destination = _graph.FirstOrDefault(x => x.Uri == envelope.Destination);
                if (destination == null)
                {
                    throw new UnknownChannelException(envelope.Destination);
                }

                return new ChannelNode[]{destination};
            }

            // TODO -- gets a LOT more sophisticated later
            var inputType = envelope.Message.GetType();
            return _graph.Where(c => c.Rules.Any(x => x.Matches(inputType)));
        }
    }
}