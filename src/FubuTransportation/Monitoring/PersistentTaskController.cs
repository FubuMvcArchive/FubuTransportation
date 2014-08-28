using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FubuCore;
using FubuCore.Logging;
using FubuCore.Util;
using FubuTransportation.Configuration;

namespace FubuTransportation.Monitoring
{
    public interface IPersistentTasks
    {
        IPersistentTask FindTask(Uri subject);
    }

    public class PersistentTaskController : ITransportPeer, IPersistentTasks
    {
        private readonly ChannelGraph _graph;
        private readonly ILogger _logger;
        private readonly ITransportPeerRepository _repository;

        private readonly ConcurrentCache<string, IPersistentTaskSource> _sources
            = new ConcurrentCache<string, IPersistentTaskSource>();

        private readonly ConcurrentCache<Uri, PersistentTaskAgent> _agents =
            new ConcurrentCache<Uri, PersistentTaskAgent>();


        private readonly Uri[] _permanentTasks;


        public PersistentTaskController(ChannelGraph graph, ILogger logger, ITransportPeerRepository repository,
            IEnumerable<IPersistentTaskSource> sources)
        {
            _graph = graph;
            _logger = logger;
            _repository = repository;
            sources.Each(x => _sources[x.Protocol] = x);

            _agents.OnMissing = uri => {
                var persistentTask = FindTask(uri);
                if (persistentTask == null) return null;

                return new PersistentTaskAgent(persistentTask);
            };

            _permanentTasks = sources.SelectMany(x => x.PermanentTasks()).ToArray();
        }

        public Task<HealthStatus> CheckStatus(Uri subject)
        {
            var agent = _agents[subject];

            if (agent == null)
            {
                return HealthStatus.Unknown.ToCompletionTask();
            }

            return checkStatus(agent);
        }

        private Task<HealthStatus> checkStatus(PersistentTaskAgent agent)
        {
            if (agent.IsActive)
            {
                // TODO -- need to do a timeout here
                return agent.AssertAvailable().ContinueWith(t => {
                    if (t.IsFaulted)
                    {
                        _logger.Error(agent.Subject, "Availability test failed for " + agent.Subject, t.Exception);
                        _logger.InfoMessage(() => new TaskAvailabilityFailed(agent.Subject));

                        return HealthStatus.Error;
                    }

                    return HealthStatus.Active;
                });
            }

            return HealthStatus.Inactive.ToCompletionTask();
        }


        public IPersistentTask FindTask(Uri subject)
        {
            if (!_sources.Has(subject.Scheme)) return null;

            var source = _sources[subject.Scheme];
            if (source == null) return null;

            return source.CreateTask(subject);
        }

        public Task StopTask(Uri subject)
        {
            var agent = _agents[subject];
            if (agent == null)
            {
                var message = "Task '{0}' is not recognized by this node".ToFormat(subject);
                return new ArgumentOutOfRangeException("subject", message).ToFaultedTask();
            }

            return agent.Deactivate().ContinueWith(t => {
                if (t.IsFaulted)
                {
                    _logger.Error(subject, "Failed to stop task " + subject, t.Exception);
                    _logger.InfoMessage(() => new FailedToStopTask(subject));
                }
                else
                {
                    _logger.InfoMessage(() => new StoppedTask(subject));
                }
            });
        }

        public void ActivateAllTasks()
        {
            var startupTasks = _permanentTasks.Select(uri => {
                return _agents[uri].Activate().ContinueWith(t => {
                    if (t.IsFaulted)
                    {
                        _logger.InfoMessage(() => new FailedToActivatePersistentTask(uri));
                        _logger.Error(uri, "Failed to activate task " + uri, t.Exception);
                        return new {Uri = uri, Success = false};
                    }

                    _logger.InfoMessage(() => new TookOwnershipOfPersistentTask(uri));

                    return new {Uri = uri, Success = true};
                });
            }).ToArray();

            Task.WaitAll(startupTasks);

            var newSubjects = startupTasks.Select(x => x.Result).Where(x => x.Success).Select(x => x.Uri);
            _repository.RecordOwnershipToThisNode(newSubjects);
        }


        public Task EnsureTasksHaveOwnership()
        {
            /* STEP 1, categorize into:
             * 1.) Tasks that I own and are active on me
             * 2.) Tasks that are active on me but supposedly owned elsewhere
             * 3.) Tasks that are owned elsewhere
             * 4.) Tasks with no owner
             * 5.) Unknown tasks
             * 
             * 
             * 
             * 
             * 
             * 
             */
            var ownedTasks = _agents
                .Where(x => x.IsActive)
                .Select(checkStatus);


            // YAGNI alert -- at this point, I'm only considering *permanent* tasks

            var owners = _repository.AllOwners();


            // check the health of all owned tasks

            // hit all owners 

            // need to check permanent tasks


            // 1., check if there is an owner already
            // 2., check if this node knows about the task
            // 3., try to start
            //    a.) If successful, log ownership
            //    b.) If failed, send failure


            throw new NotImplementedException();
        }

        // TODO -- need the peer for *this*
        public Task AssignOwnership(Uri subject, IEnumerable<ITransportPeer> peers)
        {
            throw new NotImplementedException();
            var agent = _agents[subject];
            if (agent == null)
                throw new ArgumentOutOfRangeException("subject", "Subject {0} is unknown".ToFormat(subject));

            return agent.AssignOwner(peers);
        }

        private IEnumerable<ITransportPeer> allPeers()
        {
            yield return this;
            foreach (var peer in _repository.AllPeers())
            {
                yield return peer;
            }
        } 

        public Task<OwnershipStatus> TakeOwnership(Uri subject)
        {
            var agent = _agents[subject];
            if (agent == null)
            {
                return OwnershipStatus.UnknownSubject.ToCompletionTask();
            }

            if (agent.IsActive)
            {
                return OwnershipStatus.AlreadyOwned.ToCompletionTask();
            }


            return agent.Activate().ContinueWith(t => {
                if (t.IsFaulted)
                {
                    _logger.Error(subject, "Failed to take ownership of task " + subject, t.Exception);
                    _logger.InfoMessage(() => new TaskActivationFailure(subject));

                    return OwnershipStatus.Exception;
                }

                _logger.InfoMessage(() => new TookOwnershipOfPersistentTask(subject));
                _repository.RecordOwnershipToThisNode(subject);
                return OwnershipStatus.OwnershipActivated;
            });
        }

        Task<TaskHealthResponse> ITransportPeer.CheckStatusOfOwnedTasks()
        {
            throw new NotImplementedException();
        }

        IEnumerable<Uri> ITransportPeer.CurrentlyOwnedSubjects()
        {
            throw new NotImplementedException();
        }

        string ITransportPeer.NodeId
        {
            get { return _graph.NodeId; }
        }

        string ITransportPeer.MachineName
        {
            get { return Environment.MachineName; }
        }

        IEnumerable<Uri> ITransportPeer.ReplyAddresses
        {
            get { return _graph.ReplyUriList(); }
        }


        /*
         * TODO
         * 1.) start up everything, look for immediate tasks, if owned, check the owner.  If not owned, start the election
         * 2.) Try to take ownership
         * 
         */
    }


}