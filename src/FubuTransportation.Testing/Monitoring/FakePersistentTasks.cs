using System;
using System.Collections.Generic;
using FubuCore.Util;
using FubuTransportation.Monitoring;

namespace FubuTransportation.Testing.Monitoring
{
    public class FakePersistentTasks : IPersistentTasks
    {
        public readonly Cache<Uri, FakePersistentTaskAgent> Agents =
            new Cache<Uri, FakePersistentTaskAgent>(_ => new FakePersistentTaskAgent(_));

        public IPersistentTask FindTask(Uri subject)
        {
            throw new NotImplementedException();
        }

        public IPersistentTaskAgent FindAgent(Uri subject)
        {
            return Agents[subject];
        }


        public IEnumerable<Uri> PersistentSubjects { get; set; }

        public string NodeId
        {
            get
            {
                return "FakeNode";
            }
        }
    }
}