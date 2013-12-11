using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;

namespace FubuTransportation.Subscriptions
{
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