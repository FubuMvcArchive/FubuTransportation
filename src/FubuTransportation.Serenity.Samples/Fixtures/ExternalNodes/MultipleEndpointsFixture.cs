using System;
using FubuTransportation.Configuration;
using ServiceNode;
using StoryTeller.Engine;

namespace FubuTransportation.Serenity.Samples.Fixtures.ExternalNodes
{
    public class MultipleEndpointsFixture : ExternalNodeFixture
    {
        public MultipleEndpointsFixture()
        {
            Title = "Multiple Endpoints";
        }

        [FormatAs("Send message from node {name}")]
        public void SendMessageFromNode(string name)
        {
            var node = AddTestNode<ClientRegistry>(name);
            node.Send(new SimpleMessage());
        }
    }

    public class ClientRegistry : FubuTransportRegistry<MultipleEndpointsSettings>
    {
        public ClientRegistry()
        {
            Channel(x => x.Service)
                .AcceptsMessage<MessageForClient>()
                .ReadIncoming();
        }
    }

    public class MultipleEndpointsSettings
    {
        public Uri Client { get; set; }

        public Uri Service
        {
            // A restriction with using the ExternalNodes, we have to hard-code the URI 
            // for the actual system under test.
            get { return new Uri("memory://testbus/website"); }
        }
    }

    public class MessageForClient
    {
    }
}