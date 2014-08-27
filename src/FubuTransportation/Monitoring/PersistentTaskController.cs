using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FubuCore.Logging;
using FubuCore.Util;
using FubuTransportation.Runtime.Invocation;
using FubuTransportation.Subscriptions;

namespace FubuTransportation.Monitoring
{
    // Need to or queue up requests by subject? ReaderWriterLock by subject?
    // Use an agent for each subject that uses a producer/consumer task
    public class PersistentTaskController
    {
        private readonly ILogger _logger;
        private readonly ITransportPeerRepository _repository;
        private readonly ConcurrentCache<string, IPersistentTaskSource> _sources 
            = new ConcurrentCache<string, IPersistentTaskSource>(); 

        private readonly ConcurrentCache<Uri, PersistentTaskAgent> _agents =
            new ConcurrentCache<Uri, PersistentTaskAgent>();


        private readonly Uri[] _permanentTasks;


        public PersistentTaskController(ILogger logger, ITransportPeerRepository repository, IEnumerable<IPersistentTaskSource> sources)
        {
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

            if (agent.IsActive())
            {
                return agent.AssertAvailable().ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        // TODO -- log
                        return HealthStatus.Error;
                    }

                    return HealthStatus.Active;
                });
            }

            return HealthStatus.Inactive.ToCompletionTask();

            // active, inactive, error, unknown

            throw new NotImplementedException();
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
            throw new NotImplementedException();
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

            _repository.AlterThisNode(node => {
                var newSubjects = startupTasks.Select(x => x.Result).Where(x => x.Success).Select(x => x.Uri);
                node.AddOwnership(newSubjects);
            });

            
        }

        public void EnsureTasksHaveOwnership()
        {


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


        public Task<OwnershipStatus> TakeOwnership(Uri subject)
        {
            var agent = _agents[subject];
            if (agent == null)
            {
                return OwnershipStatus.UnknownSubject.ToCompletionTask();
            }
            if (agent.IsActive())
            {
                return OwnershipStatus.AlreadyOwned.ToCompletionTask();
            }
            
            
            return agent.Activate().ContinueWith(t => {
                if (t.IsFaulted)
                {
                    // TODO -- log
                    return OwnershipStatus.Exception;
                }

                // TODO -- persist the ownership
                return OwnershipStatus.OwnershipActivated;
            });
        }



        /*
         * TODO
         * 1.) start up everything, look for immediate tasks, if owned, check the owner.  If not owned, start the election
         * 2.) Try to take ownership
         * 
         */
    }
}