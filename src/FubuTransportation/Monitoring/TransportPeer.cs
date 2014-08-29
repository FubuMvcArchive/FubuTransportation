using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FubuCore;
using FubuCore.Logging;
using FubuTransportation.Subscriptions;

namespace FubuTransportation.Monitoring
{
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
                new RequestOptions {Destination = ControlChannel})
                .ContinueWith(t => {
                    if (t.IsFaulted)
                    {
                        _logger.Error(subject, "Unable to send the TakeOwnership message to node " + _node.NodeName, t.Exception);
                        return OwnershipStatus.Exception;
                    }

                    if (!t.IsCompleted)
                    {
                        return OwnershipStatus.TimedOut;
                    }

                    return t.Result.Status;
                });
        }

        public Task<TaskHealthResponse> CheckStatusOfOwnedTasks()
        {
            var subjects = CurrentlyOwnedSubjects().ToArray();
            var request = new TaskHealthRequest
            {
                Subjects = subjects
            };

            return _serviceBus.Request<TaskHealthResponse>(request, new RequestOptions
            {
                Destination = ControlChannel,
                Timeout = 1.Minutes()
            }).ContinueWith(t => {
                if (t.IsFaulted)
                {
                    _logger.Error(NodeId, "Could not retrieve persistent status checks", t.Exception);

                    return TaskHealthResponse.ErrorFor(subjects);
                }

                if (t.IsCompleted)
                {
                    var response = t.Result;
                    response.AddMissingSubjects(subjects);

                    return response;
                }

                _logger.Info(() => "Persistent task health status timedout for node " + NodeId);
                return TaskHealthResponse.ErrorFor(subjects);
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

                    // Need to force a reload here.
                    var node = _subscriptions.FindPeer(NodeId);
                    node.RemoveOwnership(subject);
                    _subscriptions.Persist(node);
                }
                else
                {
                    _logger.Info(() => "Successfully deactivated task {0} at node {1}".ToFormat(subject, NodeId));
                }


            });
        }
    }
}