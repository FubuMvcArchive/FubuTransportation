using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Bottles.Services.Messaging.Tracking;
using FubuMVC.Core;
using FubuTransportation.Configuration;
using FubuMVC.StructureMap;
using FubuTransportation.InMemory;
using StructureMap;

namespace FubuTransportation.Testing.TestSupport
{
    public class Scenario
    {
        public readonly NodeConfiguration Website1 = new NodeConfiguration(x => x.Website1);
        public readonly NodeConfiguration Website2 = new NodeConfiguration(x => x.Website2);
        public readonly NodeConfiguration Website3 = new NodeConfiguration(x => x.Website3);
        public readonly NodeConfiguration Website4 = new NodeConfiguration(x => x.Website4);
        
        public readonly NodeConfiguration Service1 = new NodeConfiguration(x => x.Service1);
        public readonly NodeConfiguration Service2 = new NodeConfiguration(x => x.Service2);
        public readonly NodeConfiguration Service3 = new NodeConfiguration(x => x.Service3);
        public readonly NodeConfiguration Service4 = new NodeConfiguration(x => x.Service4);

        private readonly IEnumerable<NodeConfiguration> _configurations;
        private readonly IList<IScenarioStep> _steps = new List<IScenarioStep>(); 

        public Scenario()
        {
            _configurations = new NodeConfiguration[]
            {
                Website1,
                Website2,
                Website3,
                Website4,
                Service1,
                Service2,
                Service3,
                Service4
            };

            _configurations.Each(x => x.Parent = this);
        }

        internal void Execute()
        {
            InMemoryQueueManager.ClearAll();
            TestMessageRecorder.Clear();
            MessageHistory.ClearAll();

            _configurations.Each(x => x.SpinUp());
        }
    }

    public interface IScenarioStep
    {
        void PreviewAct(IScenarioWriter writer);
        void PreviewAssert(IScenarioWriter writer);

        void Act(IScenarioWriter writer);
        void Assert(IScenarioWriter writer);

        bool MatchesMessage(MessageProcessed processed);
    }

    public class HandledByStep : IScenarioStep
    {
        public HandledByStep(Message message, NodeConfiguration node)
        {
        }

        public void PreviewAct(IScenarioWriter writer)
        {
            throw new NotImplementedException();
        }

        public void PreviewAssert(IScenarioWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Act(IScenarioWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Assert(IScenarioWriter writer)
        {
            throw new NotImplementedException();
        }

        public bool MatchesMessage(MessageProcessed processed)
        {
            throw new NotImplementedException();
        }
    }

    public class RequestReplyStep<TRequest, TReply> : IScenarioStep
    {
        public RequestReplyStep()
        {
        }

        public void PreviewAct(IScenarioWriter writer)
        {
            throw new NotImplementedException();
        }

        public void PreviewAssert(IScenarioWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Act(IScenarioWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Assert(IScenarioWriter writer)
        {
            throw new NotImplementedException();
        }

        public bool MatchesMessage(MessageProcessed processed)
        {
            throw new NotImplementedException();
        }
    }


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

        public void ListensFor<T>() where T : Message
        {
            _registry.Value.Handlers.Include<SimpleHandler<T>>();
        }

        public ReplyExpression<T> Requestings<T>() where T : Message
        {
            return new ReplyExpression<T>(this);
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



        public void Dispose()
        {
            if (_runtime != null)
            {
                _runtime.Dispose();
            }
        }
    }
}