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
using System.Linq;

namespace FubuTransportation.Testing.TestSupport
{
    public class NodeConfiguration : IDisposable
    {
        private readonly Expression<Func<HarnessSettings, Uri>> _expression;
        private readonly Lazy<FubuTransportRegistry<HarnessSettings>> _registry = new Lazy<FubuTransportRegistry<HarnessSettings>>(() => FubuTransportRegistry<HarnessSettings>.Empty()); 
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

        public SendExpression<T> Sends<T>(string description) where T : Message, new()
        {
            return new SendExpression<T>(Parent, this, description);
        }

        public class SendExpression<T> where T : Message, new()
        {
            private readonly Scenario _parent;
            private readonly NodeConfiguration _sender;
            private readonly string _description;
            private readonly SendMessageStep<T> _step;

            public SendExpression(Scenario parent, NodeConfiguration sender, string description)
            {
                _parent = parent;
                _sender = sender;
                _description = description;
                _step = new SendMessageStep<T>(sender, description);
                parent.AddStep(_step);


            }

            public SendExpression<T> ShouldBeReceivedBy(NodeConfiguration node)
            {
                _step.ReceivingNodes.Add(node);
                return this;
            }

            public SendExpression<T> MatchingMessageIsReceivedBy<TMatching>(NodeConfiguration receiver) where TMatching : Message
            {
                _parent.AddStep(new MatchingMessageStep<TMatching>(_step, receiver));
                return this;
            }
        }

        public HandlesExpresion<T> Handles<T>() where T : Message
        {
            _registry.Value.Handlers.Include<SimpleHandler<T>>();
        
            return new HandlesExpresion<T>(this);
        }

        public class HandlesExpresion<T> where T : Message
        {
            private readonly NodeConfiguration _parent;

            public HandlesExpresion(NodeConfiguration parent)
            {
                parent._registry.Value.Handlers.Include<SimpleHandler<T>>();
                _parent = parent;
            }

            public HandlesExpresion<T> Raises<TResponse>() where TResponse : Message, new()
            {
                _parent._registry.Value.Handlers.Include<RequestResponseHandler<T, TResponse>>();
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

        public string Name
        {
            get
            {
                return ReflectionHelper.GetAccessor(_expression).Name;
            }
        }

        internal void Describe(IScenarioWriter writer)
        {
            if (!_registry.IsValueCreated) return;

            writer.WriteLine(Name);
            var channels = _runtime.Factory.Get<ChannelGraph>();
            using (writer.Indent())
            {
                channels.Where(x => x.Incoming)
                        .Each(x => writer.WriteLine("Listens to {0} with {1} threads", x.Uri, x.ThreadCount));
            
                //writer.BlankLine();

                channels.Where(x => x.Rules.Any()).Each(x => {
                    writer.WriteLine("Publishes to {0}", x.Key);

                    using (writer.Indent())
                    {
                        x.Rules.Each(rule => writer.Bullet(rule.Describe()));
                    }
                });

                var handlers = _runtime.Factory.Get<HandlerGraph>();
                var inputs = handlers.Select(x => x.InputType()).Where(x => x != typeof(object[]));
                if (inputs.Any())
                {
                    writer.WriteLine("Handles " + inputs.Select(x => x.Name).Join(", "));
                }

                writer.BlankLine();
            }
        }


        void IDisposable.Dispose()
        {
            if (_runtime != null)
            {
                _runtime.Dispose();
            }
        }

        public IReplyExpectation Requests<T>(string description) where T : Message, new()
        {
            return new RequestReplyExpression<T>(this, this.Parent, description);
        }
    }
}