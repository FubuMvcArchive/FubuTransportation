using System;
using FubuTestingSupport;
using FubuTransportation.Monitoring;
using FubuTransportation.Subscriptions;
using NUnit.Framework;

namespace FubuTransportation.Testing.Monitoring
{
    [TestFixture]
    public class MemoryTaskOwnershipPersistenceTester
    {
        private readonly TransportNode node1 = new TransportNode{Id = "node1", NodeName = "foo"};
        private readonly TransportNode node2 = new TransportNode{Id = "node2", NodeName = "foo"};
        private readonly TransportNode node3 = new TransportNode{Id = "node3", NodeName = "bar"};
        private readonly TransportNode node4 = new TransportNode{Id = "node4", NodeName = "bar"};

        private readonly Uri subject1 = "foo://1".ToUri();
        private readonly Uri subject2 = "foo://2".ToUri();
        private readonly Uri subject3 = "foo://3".ToUri();
        private readonly Uri subject4 = "foo://4".ToUri();
        private readonly Uri subject5 = "foo://5".ToUri();
        private readonly Uri subject6 = "foo://6".ToUri();

        [Test]
        public void persist_and_load_all_by_node_name()
        {
            var persistence = new InMemoryTaskOwnershipPersistence();

            persistence.PersistOwnership(subject1, node1);
            persistence.PersistOwnership(subject2, node1);
            persistence.PersistOwnership(subject3, node2);
            persistence.PersistOwnership(subject4, node3);
            persistence.PersistOwnership(subject5, node4);
            persistence.PersistOwnership(subject6, node4);

            persistence.All(node1.NodeName).ShouldHaveTheSameElementsAs(
                new TaskOwner{Id = subject1, Owner = node1.Id},
                new TaskOwner{Id = subject2, Owner = node1.Id},
                new TaskOwner{Id = subject3, Owner = node2.Id}
                
                );

            persistence.All(node3.NodeName).ShouldHaveTheSameElementsAs(
                new TaskOwner { Id = subject4, Owner = node3.Id },
                new TaskOwner { Id = subject5, Owner = node4.Id },
                new TaskOwner { Id = subject6, Owner = node4.Id }

                );
        }
    }
}