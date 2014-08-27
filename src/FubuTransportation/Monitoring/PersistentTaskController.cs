using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FubuCore.Util;
using FubuTransportation.Subscriptions;

namespace FubuTransportation.Monitoring
{
    // Need to or queue up requests by subject? ReaderWriterLock by subject?
    // Use an agent for each subject that uses a producer/consumer task
    public class PersistentTaskController
    {
        private readonly ITransportPeerRepository _repository;
        private readonly ConcurrentCache<string, IPersistentTaskSource> _sources 
            = new ConcurrentCache<string, IPersistentTaskSource>(); 

        private readonly ConcurrentCache<Uri, PersistentTaskAgent> _agents =
            new ConcurrentCache<Uri, PersistentTaskAgent>();


        private Uri[] _permanentTasks;


        public PersistentTaskController(ITransportPeerRepository repository, IEnumerable<IPersistentTaskSource> sources)
        {
            _repository = repository;
            sources.Each(x => _sources[x.Protocol] = x);

            _agents.OnMissing = uri => new PersistentTaskAgent(FindTask(uri));

            _permanentTasks = sources.SelectMany(x => x.PermanentTasks()).ToArray();
        }


        public IPersistentTask FindTask(Uri subject)
        {
            var source = _sources[subject.Scheme];
            if (source == null) return null;

            return source.CreateTask(subject);

            throw new NotImplementedException();
        }

        public void ActivateAllTasks()
        {
            var startupTasks = _permanentTasks.Select(uri => {
                return _agents[uri].Enqueue(t => t.Activate());
            }).ToArray();
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



            throw new NotImplementedException();
        }

        public Task<HealthStatus> CheckStatus(Uri subject)
        {
            // active, inactive, error, unknown

            throw new NotImplementedException();
        }

        /*
         * TODO
         * 1.) start up everything, look for immediate tasks, if owned, check the owner.  If not owned, start the election
         * 2.) Try to take ownership
         * 
         */
    }
}