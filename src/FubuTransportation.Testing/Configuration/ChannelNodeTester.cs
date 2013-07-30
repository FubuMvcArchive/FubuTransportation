using FubuTransportation.Configuration;
using FubuTransportation.Runtime.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using TestMessages;
using FubuTestingSupport;

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
    }
}