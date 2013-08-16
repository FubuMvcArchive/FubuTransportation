using System;
using FubuTransportation.InMemory;
using NUnit.Framework;
using FubuTestingSupport;

namespace FubuTransportation.Testing.InMemory
{
    [TestFixture]
    public class InMemorySagaRepositoryTester
    {
        [Test]
        public void find_from_empty_state()
        {
            var repository = InMemorySagaRepository<FakeState, FakeMessage>.Create();
            repository.Find(new FakeMessage{CorrelationId = Guid.NewGuid()})
                .ShouldBeNull();
        }

        [Test]
        public void save_and_find()
        {
            var repository = InMemorySagaRepository<FakeState, FakeMessage>.Create();
            var id = Guid.NewGuid();
            
            var state = new FakeState {Id = id};
            repository.Save(state);

            repository.Find(new FakeMessage {CorrelationId = id})
                      .ShouldBeTheSameAs(state);


        }

        [Test]
        public void save_and_delete_then_find_returns_null()
        {
            var repository = InMemorySagaRepository<FakeState, FakeMessage>.Create();
            var id = Guid.NewGuid();

            var state = new FakeState { Id = id };
            repository.Save(state);
            repository.Delete(state);

            repository.Find(new FakeMessage {CorrelationId = id})
                      .ShouldBeNull();
        }
    }

    public class FakeMessage
    {
        public Guid CorrelationId { get; set; }
    }

    public class FakeState
    {
        public Guid Id { get; set; }
    }
}