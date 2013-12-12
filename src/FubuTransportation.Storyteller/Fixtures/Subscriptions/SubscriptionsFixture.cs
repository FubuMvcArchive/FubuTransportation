using System.Linq;
using System.Reflection;
using Bottles.Services.Messaging.Tracking;
using FubuCore;
using FubuCore.Util;
using FubuTransportation.Configuration;
using FubuTransportation.InMemory;
using StoryTeller;
using StoryTeller.Engine;

namespace FubuTransportation.Storyteller.Fixtures.Subscriptions
{
    public class SubscriptionsFixture : Fixture
    {
        private readonly Cache<string, RunningNode> _nodes = new Cache<string, RunningNode>();
        private RunningNode _node;

        public SubscriptionsFixture()
        {
            AddSelectionValues("FubuTransportRegistries",
                Assembly.GetExecutingAssembly()
                    .ExportedTypes.Where(x => x.IsConcreteTypeOf<FubuTransportRegistry>())
                    .Select(x => x.Name)
                    .ToArray());
        }

        public override void SetUp(ITestContext context)
        {
            MessageHistory.ClearAll();
            InMemoryQueueManager.ClearAll();
            FubuTransport.ApplyMessageHistoryWatching = true;
        }

        public override void TearDown()
        {
            _nodes.Each(x => x.Dispose());
        }

        public void LoadNode(string Key, [SelectionValues("FubuTransportRegistries")] string Registry, string ReplyUri)
        {
            MessageHistory.WaitForWorkToFinish(() => {
                var node = new RunningNode(Registry, ReplyUri.ToUri());
                node.Start();

                _nodes[Key] = node;
            });
        }

        [FormatAs("The node {key}")]
        public void ForNode(string key)
        {
            _node = _nodes[key];
        }

        public IGrammar TheActiveSubscriptionsAre()
        {
            return VerifySetOf(() => _node.LoadedSubscriptions())
                .Titled("The active subscriptions are")
                .MatchOn(x => x.NodeName, x => x.MessageType, x => x.Source, x => x.Receiver);
        }

        public IGrammar ThePersistedSubscriptionsAre()
        {
            return VerifySetOf(() => _node.PersistedSubscriptions())
                .Titled("The active subscriptions are")
                .MatchOn(x => x.NodeName, x => x.MessageType, x => x.Source, x => x.Receiver);
        }
    }
}