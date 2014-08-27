using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Logging;
using FubuCore.Util;
using FubuTransportation.Monitoring;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Monitoring.PermanentTaskController
{
    [TestFixture]
    public abstract class PersistentTaskControllerContext : ITransportPeerRepository
    {
        private RecordingLogger theLogger;
        private Lazy<PersistentTaskController> _controller; 

        protected readonly Cache<string, ITransportPeer> peers =
            new Cache<string, ITransportPeer>(name => MockRepository.GenerateMock<ITransportPeer>());

        protected readonly Cache<string, FakePersistentTaskSource> sources = 
            new Cache<string, FakePersistentTaskSource>(protocol => new FakePersistentTaskSource(protocol));

        [SetUp]
        public void SetUp()
        {
            peers.ClearAll();
            sources.ClearAll();

            theLogger = new RecordingLogger();

            _controller = new Lazy<PersistentTaskController>(() => {
                return new PersistentTaskController(theLogger, this, sources);
            });

            theContextIs();
        }

        protected PersistentTaskController theController
        {
            get
            {
                return _controller.Value;
            }
        }

        protected virtual void theContextIs()
        {
            
        }

        IEnumerable<ITransportPeer> ITransportPeerRepository.AllPeers()
        {
            return peers;
        }

        IEnumerable<ITransportPeer> ITransportPeerRepository.AllOwners()
        {
            return peers.Where(x => x.CurrentlyOwnedSubjects().Any());
        }
    }
}