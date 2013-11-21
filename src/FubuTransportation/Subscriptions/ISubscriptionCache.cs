using System;
using System.Collections.Generic;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;

namespace FubuTransportation.Subscriptions
{
    public interface ISubscriptionCache
    {
        IEnumerable<ChannelNode> FindDestinationChannels(Envelope envelope);

        Uri ReplyUriFor(ChannelNode destination);
        void ReloadSubscriptions();
    }
}