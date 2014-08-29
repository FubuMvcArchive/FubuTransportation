using System.Collections.Generic;
using System.Linq;
using FubuPersistence;
using FubuTransportation.Subscriptions;
using Raven.Client;

namespace FubuTransportation.RavenDb
{
    public class RavenDbSubscriptionPersistence : ISubscriptionPersistence
    {
        private readonly ITransaction _transaction;
        private readonly IDocumentStore _store;

        public RavenDbSubscriptionPersistence(ITransaction transaction, IDocumentStore store)
        {
            _transaction = transaction;
            _store = store;
        }

        public IEnumerable<Subscription> LoadSubscriptions(string name, SubscriptionRole role)
        {
            using (var session = _store.OpenSession())
            {
                return session.Query<Subscription>()
                    .Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
                    .Where(x => x.NodeName == name && x.Role == role);
            }
        }

        public void Persist(IEnumerable<Subscription> subscriptions)
        {
            _transaction.Execute<IDocumentSession>(session => subscriptions.Each(s => session.Store(s)));
        }

        public void Persist(Subscription subscription)
        {
            _transaction.Execute<IDocumentSession>(x => x.Store(subscription));
        }

        public IEnumerable<TransportNode> NodesForGroup(string name)
        {
            using (var session = _store.OpenSession())
            {
                return session
                    .Query<TransportNode>()
                    .Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
                    .Where(x => x.NodeName == name);
            }
        }

        public void Persist(params TransportNode[] nodes)
        {
            _transaction.Execute<IDocumentSession>(x => {
                nodes.Each(node => x.Store(node));
            });
        }

        public IEnumerable<TransportNode> AllNodes()
        {
            using (var session = _store.OpenSession())
            {

                return session.Query<TransportNode>()
                    .Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
                    .ToArray();
            }
        }

        public IEnumerable<Subscription> AllSubscriptions()
        {
            using (var session = _store.OpenSession())
            {
                return session
                    .Query<Subscription>()
                    .Customize(x => x.WaitForNonStaleResultsAsOfLastWrite()).ToArray();
            }
        }

        public TransportNode LoadNode(string nodeId)
        {
            throw new System.NotImplementedException();
        }
    }
}