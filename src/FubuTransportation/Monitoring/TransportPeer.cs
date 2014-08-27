using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FubuTransportation.Subscriptions;

namespace FubuTransportation.Monitoring
{
    public class TransportPeer : ITransportPeer
    {
        private readonly TransportNode _node;
        private readonly ITaskOwnershipPersistence _persistence;
        private readonly IServiceBus _serviceBus;

        public TransportPeer(TransportNode node, ITaskOwnershipPersistence persistence, IServiceBus serviceBus)
        {
            _node = node;
            _persistence = persistence;
            _serviceBus = serviceBus;

            if (!_node.Addresses.Any())
            {
                throw new ArgumentOutOfRangeException("node", "The TransportNode must have at least one reply Uri");
            }
        }

        public Task<OwnershipStatus> TakeOwnership(Uri subject)
        {
            return _serviceBus.Request<TakeOwnershipResponse>(new TakeOwnershipRequest(subject),
                new RequestOptions {Destination = _node.Addresses.FirstOrDefault()})
                .ContinueWith(t => {
                    var ownershipStatus = t.Result.Status;

                    if (ownershipStatus == OwnershipStatus.AlreadyOwned ||
                        ownershipStatus == OwnershipStatus.OwnershipActivated)
                    {
                        _persistence.PersistOwnership(subject, _node);
                    }

                    return ownershipStatus;
                });
        }

        public Task<TaskHealthResponse> CheckStatusOfOwnedTasks()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Uri> CurrentlyOwnedSubjects()
        {
            throw new NotImplementedException();
        }

        public void RemoveOwnership(IEnumerable<Uri> subjects)
        {
            throw new NotImplementedException();
        }

        public void RemoveOwnership(Uri subject)
        {
            throw new NotImplementedException();
        }

        public string NodeId { get; private set; }
        public string MachineName { get; private set; }
        public IEnumerable<Uri> ReplyAddresses { get; private set; }
    }
}