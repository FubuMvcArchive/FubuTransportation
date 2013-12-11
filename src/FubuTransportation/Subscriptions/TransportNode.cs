using System;
using System.Collections.Generic;
using System.Linq;
using FubuTransportation.Configuration;

namespace FubuTransportation.Subscriptions
{
    public class TransportNode
    {
        private readonly IList<Uri> _addresses = new List<Uri>();

        public TransportNode()
        {
        }

        public TransportNode(ChannelGraph graph)
        {
            NodeName = graph.Name;
            Addresses = graph.ReplyUriList().ToArray();
        }

        public Guid Id { get; set; }

        public string NodeName { get; set; }

        public Uri[] Addresses
        {
            get
            {
                return _addresses.ToArray();
            }
            set
            {
                _addresses.Clear();
                if (value != null) _addresses.AddRange(value);
            }
        }

        protected bool Equals(TransportNode other)
        {
            return _addresses.OrderBy(x => x.ToString()).SequenceEqual(other._addresses.OrderBy(x => x.ToString()));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TransportNode) obj);
        }

        public override int GetHashCode()
        {
            return (_addresses != null ? _addresses.GetHashCode() : 0);
        }
    }
}