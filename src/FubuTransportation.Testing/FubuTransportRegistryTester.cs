using System;
using FubuMVC.Core;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using NUnit.Framework;
using StructureMap;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class FubuTransportRegistryTester
    {
        [SetUp]
        public void SetUp()
        {
            theRegistry = FubuTransportRegistry.Empty();
            _runtime = new Lazy<FubuRuntime>(() => {
                return FubuTransport.For(theRegistry).StructureMap(new Container()).Bootstrap();
            });

            _handlers = new Lazy<HandlerGraph>(() => _runtime.Value.Factory.Get<HandlerGraph>());
            _channels = new Lazy<ChannelGraph>(() => _runtime.Value.Factory.Get<ChannelGraph>());
        }

        private FubuTransportRegistry theRegistry;
        private Lazy<HandlerGraph> _handlers;
        private Lazy<ChannelGraph> _channels;
        private Lazy<FubuRuntime> _runtime;
    
    
        public HandlerGraph theHandlers
        {
            get { return _handlers.Value; }
        }

        public ChannelGraph theChannels
        {
            get { return _channels.Value; }
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
    }
}