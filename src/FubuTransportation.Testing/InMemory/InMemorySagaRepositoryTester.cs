using System;
using FubuMVC.Core.Runtime;
using FubuTestingSupport;
using FubuTransportation.Runtime;
using NUnit.Framework;

namespace FubuTransportation.Testing.InMemory
{
    [TestFixture]
    public class InMemorySagaRepositoryTester : InteractionContext<InMemorySagaRepository<FakeMessage>>
    {
        protected override void beforeEach()
        {
            var message = new FakeMessage();
            var fubuRequest = new InMemoryFubuRequest();
            fubuRequest.Set(message);
            Services.Inject(typeof(IFubuRequest), fubuRequest);
            var correlationIdFunc = new Func<FakeMessage, Guid>(x => x.CorrelationId);
            Services.Inject(correlationIdFunc);
        }

        [Test]
        public void can_add_new_saga_state()
        {
            ClassUnderTest.Save(new FakeSagaState {Message = "Test"});
            ClassUnderTest.Load<FakeSagaState>().Message.ShouldEqual("Test");
        }

        [Test]
        public void can_delete_saga_state()
        {
            ClassUnderTest.Save(new FakeSagaState { Message = "Test" });
            var sagaState = ClassUnderTest.Load<FakeSagaState>();
            ClassUnderTest.Delete(sagaState);
            ClassUnderTest.Load<FakeSagaState>().ShouldBeNull();
        }

        [Test]
        public void load_returns_null_when_not_found()
        {
            ClassUnderTest.Load<FakeSagaState>().ShouldBeNull();
        }
    }

    public class FakeMessage
    {
        public Guid CorrelationId { get; set; }
    }

    public class FakeSagaState
    {
        public string Message { get; set; }
    }
}