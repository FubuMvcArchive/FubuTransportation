using System;
using System.Linq;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Registration.ObjectGraph;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Registration.Conventions;
using FubuTransportation.Registration.Nodes;
using FubuTransportation.Runtime;
using NUnit.Framework;
using StructureMap;

namespace FubuTransportation.Testing.Registration.Conventions
{
    /*
    [MarkedForTermination]
    public class DefaultSagaConventionTester
    {
        private BehaviorGraph _graph;
        private HandlerGraph _handlerGraph;

        [SetUp]
        public void Setup()
        {
            var registry = new FubuRegistry();
            registry.Import<MyTestTransportRegistry>(x => x.Handlers.Include<FakeSaga>());
            registry.Policies.Add<DefaultSagaConvention>();
            registry.Services<FubuTransportServiceRegistry>();

            _graph = BehaviorGraph.BuildFrom(registry);
            _handlerGraph = _graph.Settings.Get<HandlerGraph>();
        }

        [Test]
        public void saga_node_wraps_saga_handler()
        {
            var handler = _handlerGraph.First();
            handler.OfType<SagaNode>().Count().ShouldEqual(1);
        }

        [Test]
        public void sagarepository_defaults_to_in_memory()
        {
            var handler = _handlerGraph.First();
            var sagaNode = handler.OfType<SagaNode>().First();
            var def = sagaNode.As<IContainerModel>().ToObjectDef();
            def.Dependencies.OfType<ConfiguredDependency>()
                .Any(x => x.Definition.Type == typeof(InMemorySagaRepository<SagaStarter>)).ShouldBeTrue();
        }

        [Test]
        public void sagarepository_can_be_overriden_explicitly_on_node()
        {
            var handler = _handlerGraph.First();
            var sagaNode = handler.OfType<SagaNode>().First();
            sagaNode.SagaRepositoryByAlternateId(ObjectDef.ForType<FakeRepository>());
            var def = sagaNode.As<IContainerModel>().ToObjectDef();
            def.Dependencies.OfType<ConfiguredDependency>()
                .Any(x => x.Definition.Type == typeof(FakeRepository)).ShouldBeTrue();
        }
    }
    */
    public class MyTestTransportRegistry : FubuTransportRegistry
    {
    }

    public class FakeSaga
    {
        public FakeState State { get; set; }
        public bool IsCompleted { get; set; }

        public void Initiates(SagaStarter input)
        {
        }
    }

    public class FakeState
    {
        public Guid CorrelationId { get; set; }
    }

    public class SagaStarter
    {
    }

    public class FakeRepository : ISagaRepository<SagaStarter>
    {
        public void Save<TState>(TState state) where TState : class
        {
            throw new NotImplementedException();
        }

        public TState Load<TState>() where TState : class
        {
            throw new NotImplementedException();
        }

        public void Delete<TState>(TState state) where TState : class
        {
            throw new NotImplementedException();
        }
    }
}