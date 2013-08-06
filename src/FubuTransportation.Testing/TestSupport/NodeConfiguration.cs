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