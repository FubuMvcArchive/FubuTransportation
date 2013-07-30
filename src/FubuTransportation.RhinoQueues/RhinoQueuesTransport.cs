using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using FubuCore;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using Rhino.Queues.Model;
using System.Linq;
using System.Collections.Generic;

namespace FubuTransportation.RhinoQueues
{
    public class RhinoQueuesTransport : ITransport
    {
        
        private readonly IPersistentQueues _queues;
        

        public RhinoQueuesTransport(IPersistentQueues queues)
        {
            _queues = queues;
        }

        public void Dispose()
        {
            // IPersistentQueues is disposable
        }

        // TODO -- needs hard integration tests
        public void OpenChannels(ChannelGraph graph)
        {
            var rhinoChannels = graph.Where(x => x.Protocol() == RhinoUri.Protocol).ToArray();
            
            _queues.Start(rhinoChannels.Select(x => new RhinoUri(x.Uri)));

            rhinoChannels.Each(node => {
                node.Channel = RhinoQueuesChannel.Build(new RhinoUri(node.Uri), _queues);
            });
        }

    }
}