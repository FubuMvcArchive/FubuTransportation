using System;
using System.Linq;
using FubuTransportation.Serenity.Samples.Setup;
using FubuTransportation.Serenity.Samples.SystemUnderTest;
using FubuTransportation.Serenity.Samples.SystemUnderTest.Subscriptions;
using Serenity;
using StoryTeller.Engine;

namespace FubuTransportation.Serenity.Samples.Fixtures
{
    public class MultipleEndpointsFixture : ExternalNodeFixture
    {
        public MultipleEndpointsFixture()
        {
            Title = "Multiple Endpoints";
        }

        [FormatAs("Add another service to listen for events from the system under test")]
        public void SetupAnotherService()
        {
            AddTestNode<AnotherServiceRegistry>("AnotherService");
        }

        [FormatAs("Send SomeCommand from node {name}")]
        public void SendCommandFromNode(string name)
        {
            var node = AddTestNode<ClientRegistry>(name);
            node.Send(new SomeCommand());
        }

        [FormatAs("Send message from node {name}")]
        public void SendMessageFromNode(string name)
        {
            var node = AddTestNode<ClientRegistry>(name);
            node.Send(new TestMessage());
        }

        [FormatAs("Node {name} received a response")]
        public bool NodeReceivedMessage(string name)
        {
            var node = AddTestNode<ClientRegistry>(name);
            return ShortWait(() => node.ReceivedMessage<TestResponse>());
        }

        [FormatAs("External service received the event")]
        public bool OtherServiceReceivedEvent()
        {
            var node = AddTestNode<AnotherServiceRegistry>("AnotherService");
            return ShortWait(() => node.ReceivedMessage<PublishedEvent>());
        }

        [FormatAs("The system under test should receive the message")]
        public bool SystemReceivedMessage()
        {
            var messages = Retrieve<MessageRecorder>().Messages;
            return ShortWait(() => messages.Any(x => x is TestMessage));
        }

        private bool ShortWait(Func<bool> condition)
        {
            return Wait.Until(condition, 200, 2000);
        }
    }
}