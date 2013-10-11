using System;
using System.Linq;
using System.Net;
using FubuCore;

namespace FubuTransportation.LightningQueues
{
    public class LightningUri
    {
        public static readonly string Protocol = "lq.tcp";

        private readonly Uri _address;
        private readonly int _port;
        private readonly IPEndPoint _endpoint;
        private readonly string _queueName;

        public LightningUri(string uriString) : this(new Uri(uriString))
        {
            
        }

        public LightningUri(Uri address)
        {
            if (address.Scheme != Protocol)
            {
                throw new ArgumentOutOfRangeException("{0} is the wrong protocol for a LightningQueue Uri.  Only {1} is accepted", address.Scheme, Protocol);
            }

            _address = address;
            _port = address.Port;

            if (address.Host.EqualsIgnoreCase("localhost") || address.Host.EqualsIgnoreCase(Environment.MachineName))
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

    public static class UriExtensions
    {
        public static LightningUri ToLightningUri(this Uri uri)
        {
            return new LightningUri(uri);
        }

        public static LightningUri ToLightningUri(this string uri)
        {
            return new LightningUri(uri);
        }

        public static Uri ToMachineUri(this Uri uri)
        {
            var uriString = uri.ToString().Replace("localhost", Environment.MachineName);
            return new Uri(uriString);
        }
    }
}