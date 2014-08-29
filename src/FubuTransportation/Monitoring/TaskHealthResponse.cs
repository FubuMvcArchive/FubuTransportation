using System;
using System.Collections.Generic;
using System.Linq;

namespace FubuTransportation.Monitoring
{
    public class TaskHealthResponse
    {
        public PersistentTaskStatus[] Tasks { get; set; }

        public void AddMissingSubjects(IEnumerable<Uri> subjects)
        {
            var tasks = Tasks ?? new PersistentTaskStatus[0];

            var fills = subjects.Where(x => tasks.All(_ => _.Subject != x))
                .Select(x => new PersistentTaskStatus(x, HealthStatus.Inactive));

            Tasks = tasks.Union(fills).ToArray();
        }

        public static TaskHealthResponse ErrorFor(IEnumerable<Uri> enumerable)
        {
            return new TaskHealthResponse
            {
                Tasks = enumerable.Select(x => new PersistentTaskStatus(x, HealthStatus.Error)).ToArray()
            };
        }
    }
}