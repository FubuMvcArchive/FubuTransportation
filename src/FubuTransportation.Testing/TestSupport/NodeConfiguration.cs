using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FubuCore.Reflection;
using FubuMVC.Core;
using FubuTransportation.Configuration;
using FubuMVC.StructureMap;
using FubuTransportation.InMemory;
using StructureMap;

namespace FubuTransportation.Testing.TestSupport
{
    public class NodeConfiguration : IDisposable
    {
        private readonly Expression<Func<HarnessSettings, Uri>> _expression;
        private readonly Lazy<FubuTransportRegistry<HarnessSettings>> _registry = new Lazy<FubuTransportRegistry<HarnessSettings>>(); 
        private FubuRuntime _runtime;
        private IServiceBus _serviceBus; 

        public NodeConfiguration(Expression<Func<HarnessSettings, Uri>> expression)
        {
            _expression = expression;
        }

        internal Scenario Parent { get; set; }

        internal IServiceBus ServiceBus
        {
            get { return _serviceBus; }
        }

        internal void SpinUp()
        {
            if (!_registry.IsValueCreated) return;

            var registry = _registry.Value;

            registry.Channel(_expression).ReadIncoming(2);

            registry.Handlers.Include<SourceRecordingHandler>();


            var container = new Container();

            // Make it all be 
            container.Inject(InMemoryTransport.ToInMemory<HarnessSettings>());

            _runtime = FubuTransport.For(registry).StructureMap(container).Bootstrap();
        }

        public HandlesExpresion<T> Handles<T>() where T : Message
        {
            _registry.Value.Handlers.Include<SimpleHandler<T>>();
        
            return new HandlesExpresion<T>(this);
        }

        public class HandlesExpresion<T> where T : Message
        {
            public HandlesExpresion(NodeConfiguration parent)
            {
                throw new NotImplementedException();
            }

            public HandlesExpresion<T> Raises<TResponse>() where TResponse : Message
            {
                throw new NotImplementedException();
                return this;
            } 
        }

        public ReplyExpression<T> Requesting<T>() where T : Message
        {
            return new ReplyExpression<T>(this);
        }

        public FubuTransportRegistry<HarnessSettings> Registry
        {
            get { return _registry.Value; }
        }

        public class ReplyExpression<T> where T : Message
        {
            private readonly NodeConfiguration _parent;

            public ReplyExpression(NodeConfiguration parent)
            {
                _parent = parent;
            }

            public ReplyExpression<T> RespondWith<TResponse>() where TResponse : Message, new()
            {
                _parent._registry.Value.Handlers.Include<RequestResponseHandler<T, TResponse>>();
                return this;
            } 
        }

        internal void Describe(IScenarioWriter writer)
        {
            if (!_registry.IsValueCreated) return;

            writer.WriteLine(ReflectionHelper.GetAccessor(_expression).Name);
            using (writer.Indent())
            {
                _runtime.Factory.Get<ChannelGraph>().Each(x => x.Describe(writer));
            }
        }


        void IDisposable.Dispose()
        {
            if (_runtime != null)
            {
                _runtime.Dispose();
            }
        }
    }
}