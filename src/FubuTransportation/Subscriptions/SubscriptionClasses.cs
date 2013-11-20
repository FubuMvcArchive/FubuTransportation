using System;
using System.Collections.Generic;
using System.Linq;

namespace FubuTransportation.Subscriptions
{
    /*
     * new TransportNode(ChannelGraph)
     * ChannelGraph.ToSubscriptionRequirements() : SubscriptionRequirement*
     * 
     * 
     * 
     * 
     */

    /// <summary>
    /// Sent to peer groups
    /// </summary>
    public class SubscriptionRequested
    {
        private readonly IList<Subscription> _subscriptions = new List<Subscription>();

        public Subscription[] Subscriptions
        {
            get { return _subscriptions.ToArray(); }
            set
            {
                _subscriptions.Clear();
                if (value != null) _subscriptions.AddRange(value);
            }
        }
    }

    /// <summary>
    /// Sent to "peer" nodes as a "coat check" message to refill subscriptions
    /// </summary>
    public class SubscriptionsChanged
    {
        
    }


    public interface ISubscriptionRepository
    {

    }


    public class Subscription
    {
        public Guid Id { get; set; }
        public Uri Source { get; set; }
        public Uri Receiver { get; set; }
        public string MessageType { get; set; }
        public string NodeName { get; set; }

        protected bool Equals(Subscription other)
        {
            return Equals(Source, other.Source) && Equals(Receiver, other.Receiver) && string.Equals(MessageType, other.MessageType) && string.Equals(NodeName, other.NodeName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Subscription) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Source != null ? Source.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Receiver != null ? Receiver.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (MessageType != null ? MessageType.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (NodeName != null ? NodeName.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("Source: {0}, Receiver: {1}, MessageType: {2}, NodeName: {3}", Source, Receiver, MessageType, NodeName);
        }
    }

    // Not sure this thing gets to live.
//    public class NodeGroup
//    {
//        public string NodeName { get; set; }
//
//        private readonly IList<TransportNode> _nodes = new List<TransportNode>();
//
//        public TransportNode[] Nodes
//        {
//            get
//            {
//                return _nodes.ToArray();
//            }
//            set
//            {
//                _nodes.Clear();
//                if (value != null) _nodes.AddRange(value);
//            }
//        }
//
//        public void Add(TransportNode node)
//        {
//            _nodes.Add(node);
//        }
//
//        public void Remove(TransportNode node)
//        {
//            _nodes.Remove(node);
//        }
//
//        public TransportNode FindNode(Uri uri)
//        {
//            throw new NotImplementedException();
//        }
//    }
}