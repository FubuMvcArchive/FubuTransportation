using System;
using System.Linq;
using System.Net;
using FubuCore;

namespace FubuTransportation.RhinoQueues
{
    public class RhinoUri
    {
        public static readonly string Protocol = "rhino.queues";

        private readonly Uri _address;
        private readonly int _port;
        private readonly IPEndPoint _endpoint;
        private readonly string _queueName;

        public RhinoUri(string uriString) : this(new Uri(uriString))
        {
            
        }

        public RhinoUri(Uri address)
        {
            if (address.Scheme != Protocol)
            {
                throw new ArgumentOutOfRangeException("{0} is the wrong protocol for a RhinoQueue Uri.  Only {1} is accepted", address.Scheme, Protocol);
            }

            _address = address;
            _port = address.Port;

            if (address.Host.EqualsIgnoreCase("localhost"))
            {
                _endpoint = new IPEndPoint(IPAddress.Loopback, _port);
            }
            else
            {
                _endpoint = new IPEndPoint(IPAddress.Parse(address.Host), _port);
            }

            _queueName = _address.Segments.Last();
        }

        public Uri Address
        {
            get { return _address; }
        }

        public int Port
        {
            get { return _port; }
        }

        public IPEndPoint Endpoint
        {
            get { return _endpoint; }
        }

        public string QueueName
        {
            get { return _queueName; }
        }
    }
}