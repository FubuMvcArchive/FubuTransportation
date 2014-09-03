﻿using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuCore.Util;
using FubuTransportation.Monitoring;

namespace FubuTransportation.Storyteller.Fixtures.Monitoring
{
    public class FakePersistentTaskSource : IPersistentTaskSource
    {
        private readonly Cache<Uri, FakePersistentTask> _tasks
            = new Cache<Uri, FakePersistentTask>();

        public FakePersistentTaskSource(string protocol)
        {
            Protocol = protocol;
            _tasks.OnMissing = name =>
            {
                var uri = "{0}://{1}".ToFormat(Protocol, name).ToUri();
                return new FakePersistentTask(uri);
            };
        }

        public FakePersistentTask this[string name]
        {
            get { return _tasks[name.ToUri()]; }
        }

        public FakePersistentTask this[Uri subject]
        {
            get { return _tasks[subject]; }
        }

        public string Protocol { get; private set; }

        public IEnumerable<FakePersistentTask> FakeTasks()
        {
            return _tasks;
        }

        public IEnumerable<Uri> PermanentTasks()
        {
            return _tasks.Select(x => x.Subject);
        }

        public IPersistentTask CreateTask(Uri uri)
        {
            return _tasks[uri];
        }
    }
}