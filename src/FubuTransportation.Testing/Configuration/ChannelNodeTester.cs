using System;
using FubuCore.Reflection;
using FubuMVC.Core.Runtime.Logging;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;
using FubuTransportation.Runtime.Routing;
using FubuTransportation.Scheduling;
using NUnit.Framework;
using Rhino.Mocks;
using TestMessages;
using FubuTestingSupport;
using System.Linq;

namespace FubuTransportation.Testing.Configuration
{
    [TestFixture]
    public class ChannelNodeTester
    {
        [Test]
        public void no_publishing_rules_is_always_false()
        {
            var node = new ChannelNode();
            node.Publishes(typeof(NewUser)).ShouldBeFalse();
        }

        [Test]
        public void publishes_is_true_if_any_rule_passes()
        {
            var node = new ChannelNode();
            for (int i = 0; i < 5; i++)
            {
                node.Rules.Add(MockRepository.GenerateMock<IRoutingRule>());
            }

            node.Rules[2].Stub(x => x.Matches(typeof (NewUser))).Return(true);

            node.Publishes(typeof(NewUser)).ShouldBeTrue();
        }

        [Test]
        public void publishes_is_false_if_no_rules_pass()
        {
            var node = new ChannelNode();
            for (int i = 0; i < 5; i++)
            {
                node.Rules.Add(MockRepository.GenerateMock<IRoutingRule>());
            }


            node.Publishes(typeof(NewUser)).ShouldBeFalse();
        }

        [Test]
        public void setting_address_has_to_be_a_Uri()
        {
            var node = new ChannelNode();
            Exception<ArgumentOutOfRangeException>.ShouldBeThrownBy(() => {
                node.SettingAddress = ReflectionHelper.GetAccessor<FakeThing>(x => x.Name);
            });
        }

        public void start_receiving()
        {
            if (DateTime.Today > new DateTime(2013, 11, 21))
            {
                Assert.Fail("Jeremy needs to fix the structure so that this is possible");
            }

            

//            var invoker = MockRepository.GenerateMock<IHandlerPipeline>();
//
//            var node = new ChannelNode
//            {
//                Incoming = true,
//                Channel = MockRepository.GenerateMock<IChannel>(),
//                Scheduler = new FakeScheduler()
//            };
//
//            var graph = new ChannelGraph();
//
//            var startingVisitor = new StartingChannelNodeVisitor(new Receiver(invoker, graph, node));
//            startingVisitor.Visit(node);
//            
//
//
//            node.Channel.AssertWasCalled(x => x.Receive(new Receiver(invoker, graph, node)));
        }
    }

    public class FakeScheduler : IScheduler
    {
        public void Dispose()
        {
            
        }

        public void Start(Action action)
        {
            action();
        }
    }

    [TestFixture]
    public class when_sending_an_envelope
    {
        private Envelope theEnvelope;
        private RecordingChannel theChannel;
        private ChannelNode theNode;

        [SetUp]
        public void SetUp()
        {
            theEnvelope = new Envelope()
            {
                Data = new byte[]{1,2,3,4},
                
            };

            theEnvelope.Headers["A"] = "1";
            theEnvelope.Headers["B"] = "2";
            theEnvelope.Headers["C"] = "3";

            theChannel = new RecordingChannel();

            theNode = new ChannelNode
            {
                Channel = theChannel,
                Key = "Foo",
                Uri = "foo://bar".ToUri()
            };

            theNode.Send(theEnvelope);
        }

        [Test]
        public void should_have_sent_the_data()
        {
            theChannel.Sent.Single().Data.ShouldEqual(theEnvelope.Data);
        }

        [Test]
        public void should_have_sent_a_copy_of_the_headers()
        {
            var sentHeaders = theChannel.Sent.Single().Headers;
            sentHeaders.ShouldNotBeTheSameAs(theEnvelope.Headers);

            sentHeaders["A"].ShouldEqual("1");
            sentHeaders["B"].ShouldEqual("2");
            sentHeaders["C"].ShouldEqual("3");
        }

        [Test]
        public void sends_the_channel_key()
        {
            var sentHeaders = theChannel.Sent.Single().Headers;
            sentHeaders[Envelope.ChannelKey].ShouldEqual(theNode.Key);
        }

        [Test]
        public void sends_the_destination_as_a_header()
        {
            var sentHeaders = theChannel.Sent.Single().Headers;
            sentHeaders[Envelope.DestinationKey].ToUri().ShouldEqual(theNode.Uri);
        }
    }

    public class FakeThing
    {
        public string Name { get; set; }
    }
}