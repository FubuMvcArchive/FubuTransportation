using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FubuCore;
using FubuCore.DependencyAnalysis;
using FubuCore.Logging;
using FubuTransportation.Subscriptions;

namespace FubuTransportation.Monitoring
{

    // TODO -- add quite a bit of logging throughout this thing
    public class TransportPeer : ITransportPeer
    {
        private readonly TransportNode _node;
        private readonly ISubscriptionRepository _subscriptions;
        private readonly IServiceBus _serviceBus;
        private readonly ILogger _logger;

        public TransportPeer(TransportNode node, ISubscriptionRepository subscriptions, IServiceBus serviceBus, ILogger logger)
        {
            _node = node;
            _subscriptions = subscriptions;
            _serviceBus = serviceBus;
            _logger = logger;

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
                        _node.AddOwnership(subject);
                        _subscriptions.Persist(_node);
                    }

                    return ownershipStatus;
                });
        }

        public Task<TaskHealthResponse> CheckStatusOfOwnedTasks()
        {
            // TODO -- ERRORS DO NOT COME OUT OF HERE AT ALL
            // TODO -- need to timeout
            // TODO -- if it times out or faults, return
            // a status saying that it's all bad
            // Needs to fill out with subjects that aren't coming from the node
            throw new NotImplementedException("Not tested");
            var request = new TaskHealthRequest
            {
                Subjects = CurrentlyOwnedSubjects().ToArray()
            };

            return _serviceBus.Request<TaskHealthResponse>(request, new RequestOptions
            {
                // TODO -- this is smelly. Introduce the idea of a "control" queue?
                Destination = ControlChannel
            });
        }

        public IEnumerable<Uri> CurrentlyOwnedSubjects()
        {
            return _node.OwnedTasks;
        }


        public string NodeId
        {
            get
            {
                return _node.Id;
            }
        }

        public string MachineName
        {
            get
            {
                return _node.MachineName;
            }
        }

        public Uri ControlChannel
        {
            get
            {
                return _node.Addresses.FirstOrDefault();
            }
        }

        public Task Deactivate(Uri subject)
        {
            _logger.Info(() => "Requesting a deactivation of task {0} at node {1}".ToFormat(subject, NodeId));

            return _serviceBus.Request<TaskDeactivationResponse>(new TaskDeactivation(subject), new RequestOptions
            {
                Destination = ControlChannel,
                Timeout = 1.Minutes()
            }).ContinueWith(t => {
                if (t.IsFaulted)
                {
                    _logger.Error(subject, "Failed while trying to deactivate a remote task", t.Exception);
                }
                else
                {
                    _logger.Info(() => "Successfully deactivated task {0} at node {1}".ToFormat(subject, NodeId));
                }

                // Need to force a reload here.
                var node = _subscriptions.FindPeer(NodeId);
                node.RemoveOwnership(subject);
                _subscriptions.Persist(node);
            });
        }
    }
}