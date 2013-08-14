using System;
using System.Collections.Generic;
using System.Linq;
using FubuTransportation.Configuration;

namespace FubuTransportation.Runtime
{
    public class Subscriptions : ISubscriptions, IDisposable
    {
        private readonly ChannelGraph _graph;

        public Subscriptions(ChannelGraph graph)
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

        public void Dispose()
        {
            _graph.Each(x => x.Channel.Dispose());
        }
    }
}