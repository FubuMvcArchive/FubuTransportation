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
        IPersistentTaskAgent FindAgent(Uri subject);
        IEnumerable<Uri> PersistentSubjects { get; }

    }

    public interface IPersistentTaskController
    {
        Task<HealthStatus> CheckStatus(Uri subject);
        Task<bool> Deactivate(Uri subject);
        void ActivateAllTasks();
        Task EnsureTasksHaveOwnership();
        Task<OwnershipStatus> TakeOwnership(Uri subject);
        Task<TaskHealthResponse> CheckStatusOfOwnedTasks();
        IEnumerable<Uri> ActiveTasks();
    }

    public class PersistentTaskController : ITransportPeer, IPersistentTasks, IPersistentTaskController
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

        // TODO -- make this thing time out!!!!!!!!!
        private Task<HealthStatus> checkStatus(PersistentTaskAgent agent)
        {
            if (agent.IsActive)
            {
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

        public IPersistentTaskAgent FindAgent(Uri subject)
        {
            return _agents[subject];
        }

        IEnumerable<Uri> IPersistentTasks.PersistentSubjects
        {
            get { return _permanentTasks; }
        }

        public Task<bool> Deactivate(Uri subject)
        {
            var agent = _agents[subject];
            if (agent == null)
            {
                _logger.Info("Task '{0}' is not recognized by this node".ToFormat(subject));

                return false.ToCompletionTask();
            }

            return agent.Deactivate().ContinueWith(t => {
                if (t.IsFaulted)
                {
                    _logger.Error(subject, "Failed to stop task " + subject, t.Exception);
                    _logger.InfoMessage(() => new FailedToStopTask(subject));

                    return false;
                }


                _repository.RemoveOwnershipFromThisNode(subject);
                _logger.InfoMessage(() => new StoppedTask(subject));

                return true;
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
            using (var router = new HealthAndAssignmentRouter(_logger,this, allPeers().ToArray()))
            {
                return router.EnsureAllTasksAreAssignedAndRunning();
            }
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

        public Task<TaskHealthResponse> CheckStatusOfOwnedTasks()
        {
            var checks = CurrentlyOwnedSubjects()
                .Select(subject => CheckStatus(subject).ContinueWith(t => new PersistentTaskStatus(subject, t.Result)))
                .ToArray();

            return Task.Factory.ContinueWhenAll(checks, tasks => new TaskHealthResponse
            {
                Tasks = tasks.Select(x => x.Result).ToArray()
            });
        }

        public IEnumerable<Uri> ActiveTasks()
        {
            return _agents.Where(x => x.IsActive).Select(x => x.Subject).ToArray();
        }

        public IEnumerable<Uri> CurrentlyOwnedSubjects()
        {
            var activeTasks = _agents.Where(x => x.IsActive).Select(x => x.Subject);
            return
                _repository.LocalNode().OwnedTasks.Union(activeTasks).ToArray();
        }

        string ITransportPeer.NodeId
        {
            get { return _graph.NodeId; }
        }

        string ITransportPeer.MachineName
        {
            get { return Environment.MachineName; }
        }

        // TODO -- think this should be explicitly set later
        public Uri ControlChannel
        {
            get
            {
                return _graph.ReplyUriList().FirstOrDefault();
            }
        }

    }


}