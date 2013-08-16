using System;
using FubuMVC.Core.Registration.Nodes;
using FubuTransportation.Configuration;
using FubuTransportation.Registration.Nodes;
using FubuTransportation.Sagas;
using FubuTransportation.Testing.ScenarioSupport;
using NUnit.Framework;
using FubuTestingSupport;

namespace FubuTransportation.Testing.Sagas
{
    [TestFixture]
    public class StatefulSagaConventionTester
    {
        [Test]
        public void is_saga_handler_positive()
        {
            StatefulSagaConvention.IsSagaHandler(HandlerCall.For<SimpleSagaHandler>(x => x.Start(null)))
                .ShouldBeTrue();

            StatefulSagaConvention.IsSagaHandler(HandlerCall.For<SimpleSagaHandler>(x => x.Second(null)))
                .ShouldBeTrue();

            StatefulSagaConvention.IsSagaHandler(HandlerCall.For<SimpleSagaHandler>(x => x.Last(null)))
                .ShouldBeTrue();
        }

        [Test]
        public void is_saga_handler_negative()
        {
            StatefulSagaConvention.IsSagaHandler(HandlerCall.For<SimpleHandler<OneMessage>>(x => x.Handle(null)))
                .ShouldBeFalse();
        }

        [Test]
        public void is_saga_chain_is_false_for_regular_behavior_chain()
        {
            StatefulSagaConvention.IsSagaChain(new BehaviorChain())
                .ShouldBeFalse();
        }

        [Test]
        public void is_saga_chain_is_false_for_handler_chain_with_no_saga_handlers()
        {
            var call = HandlerCall.For<SimpleHandler<OneMessage>>(x => x.Handle(null));

            var chain = new HandlerChain();
            chain.AddToEnd(call);

            StatefulSagaConvention.IsSagaChain(chain)
                .ShouldBeFalse();
        }

        [Test]
        public void is_saga_chain_is_true_for_handler_chain_with_a_saga_handler()
        {
            var call = HandlerCall.For<SimpleSagaHandler>(x => x.Last(null));

            var chain = new HandlerChain();
            chain.AddToEnd(call);

            StatefulSagaConvention.IsSagaChain(chain)
                .ShouldBeTrue();
        }

        [Test]
        public void to_saga_types_for_a_handler_call()
        {
            var call = HandlerCall.For<SimpleSagaHandler>(x => x.Last(null));
            var types = StatefulSagaConvention.ToSagaTypes(call);

            types.HandlerType.ShouldEqual(typeof (SimpleSagaHandler));
            types.MessageType.ShouldEqual(typeof (SagaMessageThree));
            types.StateType.ShouldEqual(typeof (MySagaState));
        }

        [Test]
        public void saga_types_being_able_to_gimme_a_correlation_id_getter_from_the_message_type()
        {
            var types = new SagaTypes
            {
                MessageType = typeof (SagaMessageOne)
            };

            var func = types.ToCorrelationIdFunc().ShouldBeOfType<Func<SagaMessageOne, Guid>>();

            var message = new SagaMessageOne
            {
                CorrelationId = Guid.NewGuid()
            };

            func(message).ShouldEqual(message.CorrelationId);
        }

        [Test]
        public void saga_types_being_able_to_gimme_an_id_getter_for_the_state_object()
        {
            var types = new SagaTypes
            {
                MessageType = typeof(SagaMessageOne),
                StateType = typeof(MySagaState)
            };

            var func = types.ToSagaIdFunc().ShouldBeOfType<Func<MySagaState, Guid>>();

            var state = new MySagaState
            {
                Id = Guid.NewGuid()
            };

            func(state).ShouldEqual(state.Id);

        }

        [Test]
        public void saga_types_matches_idiom()
        {
            new SagaTypes
            {
                MessageType = typeof(SagaMessageOne),
                StateType = typeof(MySagaState)
            }.MatchesStateIdAndMessageCorrelationIdIdiom().ShouldBeTrue();
        }

        [Test]
        public void saga_types_does_not_match_idiom_because_of_state_type_not_having_id()
        {
            new SagaTypes
            {
                MessageType = typeof(SagaMessageOne),
                StateType = GetType()
            }.MatchesStateIdAndMessageCorrelationIdIdiom().ShouldBeFalse();
        }

        [Test]
        public void saga_types_does_not_match_idiom_because_of_message_type_not_having_correlation_id()
        {
            new SagaTypes
            {
                MessageType = GetType(),
                StateType = typeof(MySagaState)
            }.MatchesStateIdAndMessageCorrelationIdIdiom().ShouldBeFalse();
        }
    }

    public class MySagaState
    {
        public Guid Id { get; set; }
    }

    public class SagaMessageOne
    {
        public Guid CorrelationId { get; set; }
    }

    public class SagaMessageTwo
    {
        public Guid CorrelationId { get; set; }
    }

    public class SagaMessageThree
    {
        public Guid CorrelationId { get; set; }
    }

    public class SimpleSagaHandler : IStatefulSaga<MySagaState>
    {
        public bool IsCompleted()
        {
            throw new NotImplementedException();
        }

        public MySagaState State { get; set; }

        public void Start(SagaMessageOne one)
        {
            
        }

        public void Second(SagaMessageTwo two)
        {
            
        }

        public void Last(SagaMessageThree three)
        {
            
        }
    }
}