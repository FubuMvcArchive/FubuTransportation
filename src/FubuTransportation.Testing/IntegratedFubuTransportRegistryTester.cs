using System;
using System.Linq.Expressions;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Routing;
using NUnit.Framework;
using System.Linq;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class IntegratedFubuTransportRegistryTester
    {
        [SetUp]
        public void SetUp()
        {
            theRegistry = new BusRegistry();
            _behaviors = new Lazy<BehaviorGraph>(() => {
                return BehaviorGraph.BuildFrom(registry => {
                    new FubuTransportationExtensions().As<IFubuRegistryExtension>().Configure(registry);
                    theRegistry.As<IFubuRegistryExtension>().Configure(registry);
                });
            });

            _handlers = new Lazy<HandlerGraph>(() => _behaviors.Value.Settings.Get<HandlerGraph>());
            _channels = new Lazy<ChannelGraph>(() => _behaviors.Value.Settings.Get<ChannelGraph>());
        }

        private BusRegistry theRegistry;
        private Lazy<HandlerGraph> _handlers;
        private Lazy<ChannelGraph> _channels;
        private Lazy<BehaviorGraph> _behaviors;


        public HandlerGraph theHandlers
        {
            get { return _handlers.Value; }
        }

        public ChannelGraph theChannels
        {
            get { return _channels.Value; }
        }

        public ChannelNode channelFor(Expression<Func<BusSettings, Uri>> expression)
        {
            return theChannels.ChannelFor(expression);
        }

        [Test]
        public void set_channel_to_listening()
        {
            theRegistry.Channel(x => x.Upstream).Incoming();

            channelFor(x => x.Upstream).Incoming.ShouldBeTrue();
            channelFor(x => x.Downstream).Incoming.ShouldBeFalse();
        }


        [Test]
        public void set_the_default_content_type_by_serializer_type()
        {
            theRegistry.DefaultSerializer<BinarySerializer>();

            theChannels.DefaultContentType.ShouldEqual(new BinarySerializer().ContentType);
        }

        [Test]
        public void set_the_default_content_type_by_string()
        {
            theRegistry.DefaultContentType("application/json");
            theChannels.DefaultContentType.ShouldEqual("application/json");
        }

        [Test]
        public void set_the_default_content_type_for_a_channel_by_serializer()
        {
            theRegistry.Channel(x => x.Outbound).DefaultSerializer<BinarySerializer>();
            theRegistry.Channel(x => x.Downstream).DefaultSerializer<XmlMessageSerializer>();
            theRegistry.Channel(x => x.Upstream).DefaultSerializer<BasicJsonMessageSerializer>();

            channelFor(x => x.Outbound).DefaultContentType.ShouldEqual(new BinarySerializer().ContentType);
            channelFor(x => x.Downstream).DefaultContentType.ShouldEqual(new XmlMessageSerializer().ContentType);
            channelFor(x => x.Upstream).DefaultContentType.ShouldEqual(new BasicJsonMessageSerializer().ContentType);
        }

        [Test]
        public void set_the_default_content_type_for_a_channel_by_string()
        {
            theRegistry.Channel(x => x.Outbound).DefaultContentType("application/json");
            channelFor(x => x.Outbound).DefaultContentType.ShouldEqual("application/json");
        }

        [Test]
        public void add_namespace_publishing_rule()
        {
            theRegistry.Channel(x => x.Outbound).PublishesMessagesInNamespaceContainingType<BusSettings>();

            channelFor(x => x.Outbound).Rules.Single()
                                       .ShouldEqual(NamespaceRule.For<BusSettings>());

            channelFor(x => x.Upstream).Rules.Any().ShouldBeFalse();
        }

        [Test]
        public void add_namespace_publishing_rule_2()
        {
            theRegistry.Channel(x => x.Outbound).PublishesMessagesInNamespace(typeof(BusSettings).Namespace);

            channelFor(x => x.Outbound).Rules.Single()
                                       .ShouldEqual(NamespaceRule.For<BusSettings>());

            channelFor(x => x.Upstream).Rules.Any().ShouldBeFalse();
        }

        [Test]
        public void add_assembly_publishing_rule()
        {
            theRegistry.Channel(x => x.Outbound).PublishesMessagesInAssemblyContainingType<BusSettings>();

            channelFor(x => x.Outbound).Rules.Single()
                                       .ShouldEqual(AssemblyRule.For<BusSettings>());

            channelFor(x => x.Upstream).Rules.Any().ShouldBeFalse();
        }

        [Test]
        public void add_assembly_publishing_rule_2()
        {
            theRegistry.Channel(x => x.Outbound).PublishesMessagesInAssembly(typeof(BusSettings).Assembly.GetName().Name);

            channelFor(x => x.Outbound).Rules.Single()
                                       .ShouldEqual(AssemblyRule.For<BusSettings>());

            channelFor(x => x.Upstream).Rules.Any().ShouldBeFalse();
        }

        [Test]
        public void add_single_type_rule()
        {
            theRegistry.Channel(x => x.Outbound).PublishesMessage<BusSettings>();
            channelFor(x => x.Outbound).Rules.Single()
                                       .ShouldBeOfType<SingleTypeRoutingRule<BusSettings>>();

            channelFor(x => x.Upstream).Rules.Any().ShouldBeFalse();
        }

        [Test]
        public void add_single_type_rule_2()
        {
            theRegistry.Channel(x => x.Outbound).PublishesMessage(typeof(BusSettings));
            channelFor(x => x.Outbound).Rules.Single()
                                       .ShouldBeOfType<SingleTypeRoutingRule<BusSettings>>();

            channelFor(x => x.Upstream).Rules.Any().ShouldBeFalse();
        }

        [Test]
        public void add_adhoc_rule()
        {
            theRegistry.Channel(x => x.Outbound).PublishesMessages(type => {
                return type == typeof (BusSettings);
            });

            var rule = channelFor(x => x.Outbound).Rules.Single();

            rule.ShouldBeOfType<LambdaRoutingRule>();
            rule.Matches(typeof(BusSettings)).ShouldBeTrue();
            rule.Matches(GetType()).ShouldBeFalse();
        }

        // TODO -- set thread count for listening
    }

    public class BusSettings
    {
        public Uri Outbound { get; set; }
        public Uri Downstream { get; set; }
        public Uri Upstream { get; set; }
    }

    public class BusRegistry : FubuTransportRegistry<BusSettings>
    {
    }
}