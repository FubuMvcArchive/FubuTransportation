using System.Collections.Generic;
using System.Linq;
using FubuPersistence;
using FubuTransportation.ScheduledJobs;
using FubuTransportation.ScheduledJobs.Persistence;
using Lucene.Net.Documents;
using Raven.Client;

namespace FubuTransportation.RavenDb
{
    public class RavenDbSchedulePersistence : ISchedulePersistence
    {
        private readonly ITransaction _transaction;
        private readonly IDocumentStore _store;
        private int _maxHistory;

        public RavenDbSchedulePersistence(ITransaction transaction, IDocumentStore store, ScheduledJobGraph jobs)
        {
            _transaction = transaction;
            _store = store;
            _maxHistory = jobs.MaxJobExecutionRecordsToKeepInHistory;
        }

        public IEnumerable<JobStatusDTO> FindAll(string nodeName)
        {
            using (var session = _store.OpenSession())
            {
                return session
                    .Query<JobStatusDTO>()
                    .Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
                    .Where(x => x.NodeName == nodeName)
                    .ToArray();
            }
        }

        public IEnumerable<JobStatusDTO> FindAllActive(string nodeName)
        {
            using (var session = _store.OpenSession())
            {
                return session
                    .Query<JobStatusDTO>()
                    .Customize(x => x.WaitForNonStaleResultsAsOfLastWrite())
                    .Where(x => x.NodeName == nodeName)
                    .Where(x => x.Status != JobExecutionStatus.Inactive)
                    .ToArray();
            }
        }

        public void Persist(IEnumerable<JobStatusDTO> statuses)
        {
            _transaction.Execute<IDocumentSession>(session => {
                statuses.Each(x => session.Store(x));
            });
        }

        public void Persist(JobStatusDTO status)
        {
            _transaction.Execute<IDocumentSession>(session =>
            {
                session.Store(status);
            });
        }

        public JobStatusDTO Find(string nodeName, string jobKey)
        {
            var id = JobStatusDTO.ToId(nodeName, jobKey);
            using (var session = _store.OpenSession())
            {
                return session.Load<JobStatusDTO>(id);
            }
        }


        public void RecordHistory(string nodeName, string jobKey, JobExecutionRecord record)
        {
            var id = JobStatusDTO.ToId(nodeName, jobKey);

            _transaction.Execute<IDocumentSession>(session => {
                var status = session.Load<JobStatusDTO>(id);
                if (status != null)
                {
                    status.History.Append(record, _maxHistory);
                    session.Store(status);
                }
            });
        }

        public IEnumerable<JobExecutionRecord> FindHistory(string nodeName, string jobKey)
        {
            var id = JobStatusDTO.ToId(nodeName, jobKey);
            using (var session = _store.OpenSession())
            {
                var status = session.Load<JobStatusDTO>(id);
                return status == null ? new JobExecutionRecord[0] : status.History.Records.ToArray();
            }
        }
    }
}