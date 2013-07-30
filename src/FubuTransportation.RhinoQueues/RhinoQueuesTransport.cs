using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using FubuCore;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using Rhino.Queues.Model;
using System.Linq;

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
            throw new NotImplementedException();
        }

        public void OpenChannels(ChannelGraph graph)
        {
            throw new NotImplementedException();
        }

        public bool Matches(Uri uri)
        {
            return uri.Scheme.EqualsIgnoreCase("rhino.queues");
        }


    }
}