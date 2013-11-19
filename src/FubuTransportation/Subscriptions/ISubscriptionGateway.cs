using System;
using System.Collections.Generic;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;

namespace FubuTransportation.Subscriptions
{
    public interface ISubscriptionGateway
    {
        IEnumerable<ChannelNode> FindChannels(Envelope envelope);
        void Start();

        Uri ReplyUriFor(ChannelNode destination);
    }
}