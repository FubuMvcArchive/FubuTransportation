using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuCore.Util;
using FubuTestingSupport;
using FubuTransportation.Monitoring;
using NUnit.Framework;

namespace FubuTransportation.Testing.Monitoring.HealthAndAssignmentRouter
{
    [TestFixture]
    public class HealthAndAssignmentRouterTester
    {
        private readonly Uri subject1 = "subject://1".ToUri();
        private readonly Uri subject2 = "subject://2".ToUri();
        private readonly Uri subject3 = "subject://3".ToUri();
        private readonly Uri subject4 = "subject://4".ToUri();

        private readonly Cache<Uri, FakeTransportPeer> ExpectedOwnership
            = new Cache<Uri, FakeTransportPeer>();

        private FakePersistentTasks theTasks;
        private FakeTransportPeer peer1;
        private FakeTransportPeer peer2;
        private FakeTransportPeer peer3;
        private FakeTransportPeer peer4;
        private FubuTransportation.Monitoring.HealthAndAssignmentRouter theRouter;
        private FakeTransportPeer[] peers;

        [SetUp]
        public void SetUp()
        {
            peer1 = new FakeTransportPeer("Peer1", Environment.MachineName, "control://1".ToUri());
            peer2 = new FakeTransportPeer("Peer2", Environment.MachineName, "control://2".ToUri());
            peer3 = new FakeTransportPeer("Peer3", Environment.MachineName, "control://3".ToUri());
            peer4 = new FakeTransportPeer("Peer4", Environment.MachineName, "control://4".ToUri());

            ExpectedOwnership.ClearAll();

            theTasks = new FakePersistentTasks();
            peers = new FakeTransportPeer[] {peer1, peer2, peer3, peer4};
            theRouter = new FubuTransportation.Monitoring.HealthAndAssignmentRouter(theTasks, peers);
        }

        public void assertPeerAssignments()
        {
            theRouter.EnsureAllTasksAreAssignedAndRunning().Wait();

            var list = new List<string>();

            ExpectedOwnership.Each((subject, peer) => {
                if (peer.OwnedSubjects.Contains(subject)) return;


                var actual = peers.FirstOrDefault(x => x.OwnedSubjects.Contains(subject));

                if (actual == null)
                {
                    list.Add("Subject '{0}' should have been owned by node '{1}', but was not assigned".ToFormat(
                        subject, peer.NodeId));
                }
                else
                {
                    list.Add("Subject '{0}' should have been owned by node '{1}', but was assigned to '{2}'".ToFormat(
                        subject, peer.NodeId, actual.NodeId));
                }
            });

            if (list.Any())
            {
                Assert.Fail(list.Join("\n"));
            }
        }

        [Test]
        public void assign_a_single_job_that_is_not_already_assigned()
        {
            theTasks.PersistentSubjects = new Uri[] {subject1};
            theTasks.Agents[subject1].PeerIdToSelect = peer3.NodeId;

            ExpectedOwnership[subject1] = peer3;

            assertPeerAssignments();
        }

        [Test]
        public void assign_multiple_jobs_that_are_not_already_owned()
        {
            theTasks.PersistentSubjects = new Uri[] {subject1, subject2, subject3, subject4};

            theTasks.Agents[subject1].PeerIdToSelect = peer1.NodeId;
            theTasks.Agents[subject2].PeerIdToSelect = peer2.NodeId;
            theTasks.Agents[subject3].PeerIdToSelect = peer3.NodeId;
            theTasks.Agents[subject4].PeerIdToSelect = peer4.NodeId;

            ExpectedOwnership[subject1] = peer1;
            ExpectedOwnership[subject2] = peer2;
            ExpectedOwnership[subject3] = peer3;
            ExpectedOwnership[subject4] = peer4;

            assertPeerAssignments();
        }

        [Test]
        public void all_jobs_are_assigned_and_active_do_nothing()
        {
            theTasks.PersistentSubjects = new Uri[] {subject1, subject2, subject3, subject4};


            peer3.IsSuccessfullyRunning(subject1);
            peer3.IsSuccessfullyRunning(subject2);
            peer4.IsSuccessfullyRunning(subject3);
            peer4.IsSuccessfullyRunning(subject4);

            // nothing is reassigned
            ExpectedOwnership[subject1] = peer3;
            ExpectedOwnership[subject2] = peer3;
            ExpectedOwnership[subject3] = peer4;
            ExpectedOwnership[subject4] = peer4;

            assertPeerAssignments();
        }

        [Test]
        public void a_job_is_owned_in_active_state()
        {
            theTasks.PersistentSubjects = new[] {subject1};

            peer3.OwnsButIsInState(subject1, HealthStatus.Active);
            theTasks.Agents[subject1].PeerIdToSelect = peer4.NodeId;

            // should NOT be reassigned
            ExpectedOwnership[subject1] = peer3;

            assertPeerAssignments();
        }

        [Test]
        public void a_job_is_owned_but_in_error_state()
        {
            theTasks.PersistentSubjects = new[] {subject1};

            peer3.OwnsButIsInState(subject1, HealthStatus.Error);
            theTasks.Agents[subject1].PeerIdToSelect = peer4.NodeId;

            // should be reassigned
            ExpectedOwnership[subject1] = peer4;

            assertPeerAssignments();
        }


        [Test]
        public void a_job_is_owned_but_in_unknown_state()
        {
            theTasks.PersistentSubjects = new[] {subject1};

            peer3.OwnsButIsInState(subject1, HealthStatus.Unknown);
            theTasks.Agents[subject1].PeerIdToSelect = peer4.NodeId;

            // should be reassigned
            ExpectedOwnership[subject1] = peer4;

            assertPeerAssignments();
        }

        [Test]
        public void a_job_is_owned_but_in_timed_out_state()
        {
            theTasks.PersistentSubjects = new[] { subject1 };

            peer3.OwnsButIsInState(subject1, HealthStatus.Timedout);
            theTasks.Agents[subject1].PeerIdToSelect = peer4.NodeId;

            // should be reassigned
            ExpectedOwnership[subject1] = peer4;

            assertPeerAssignments();
        }


        [Test]
        public void mixed_healthy_and_unhealthy_tasks()
        {
            theTasks.PersistentSubjects = new Uri[] {subject1, subject2, subject3, subject4};


            peer3.IsSuccessfullyRunning(subject1);
            peer3.IsSuccessfullyRunning(subject2);
            peer4.OwnsButIsInState(subject3, HealthStatus.Error);
            peer4.OwnsButIsInState(subject4, HealthStatus.Error);

            theTasks.Agents[subject3].PeerIdToSelect = peer1.NodeId;
            theTasks.Agents[subject4].PeerIdToSelect = peer2.NodeId;

            // nothing is reassigned
            ExpectedOwnership[subject1] = peer3;
            ExpectedOwnership[subject2] = peer3;

            // these two jobs should be reassigned
            ExpectedOwnership[subject3] = peer1;
            ExpectedOwnership[subject4] = peer2;

            assertPeerAssignments();
        }

        [Test]
        public void mixed_assigned_and_un_assigned_tasks()
        {
            theTasks.PersistentSubjects = new Uri[] {subject1, subject2, subject3, subject4};


            peer3.IsSuccessfullyRunning(subject1);
            peer3.IsSuccessfullyRunning(subject2);

            theTasks.Agents[subject3].PeerIdToSelect = peer1.NodeId;
            theTasks.Agents[subject4].PeerIdToSelect = peer2.NodeId;

            // nothing is reassigned
            ExpectedOwnership[subject1] = peer3;
            ExpectedOwnership[subject2] = peer3;

            // these two jobs should be reassigned
            ExpectedOwnership[subject3] = peer1;
            ExpectedOwnership[subject4] = peer2;

            assertPeerAssignments();
        }


        [Test]
        public void when_reassigning_a_job_sends_a_deactivation_message_to_that_peer()
        {
            theTasks.PersistentSubjects = new[] { subject1 };

            peer3.OwnsButIsInState(subject1, HealthStatus.Error);
            theTasks.Agents[subject1].PeerIdToSelect = peer4.NodeId;

            // should be reassigned
            ExpectedOwnership[subject1] = peer4;

            assertPeerAssignments();

            // should have been removed
            peer3.OwnedSubjects.ShouldNotContain(subject1);
        }
    }
}