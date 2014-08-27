using System;
using System.Collections.Generic;
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
        }

        public Task<TakeOwnershipResponse> TakeOwnership(IEnumerable<Uri> subject)
        {
            throw new NotImplementedException();
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