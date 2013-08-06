using System.Collections.Generic;
using Bottles.Services.Messaging.Tracking;
using FubuTransportation.InMemory;
using System.Linq;

namespace FubuTransportation.Testing.TestSupport
{
    public class Scenario
    {
        public readonly NodeConfiguration Website1 = new NodeConfiguration(x => x.Website1);
        public readonly NodeConfiguration Website2 = new NodeConfiguration(x => x.Website2);
        public readonly NodeConfiguration Website3 = new NodeConfiguration(x => x.Website3);
        public readonly NodeConfiguration Website4 = new NodeConfiguration(x => x.Website4);
        
        public readonly NodeConfiguration Service1 = new NodeConfiguration(x => x.Service1);
        public readonly NodeConfiguration Service2 = new NodeConfiguration(x => x.Service2);
        public readonly NodeConfiguration Service3 = new NodeConfiguration(x => x.Service3);
        public readonly NodeConfiguration Service4 = new NodeConfiguration(x => x.Service4);

        private readonly IEnumerable<NodeConfiguration> _configurations;
        private readonly IList<IScenarioStep> _steps = new List<IScenarioStep>(); 

        public Scenario()
        {
            // TODO -- derive title from the class name? Strip out the "_"'s?

            _configurations = new NodeConfiguration[]
            {
                Website1,
                Website2,
                Website3,
                Website4,
                Service1,
                Service2,
                Service3,
                Service4
            };

            _configurations.Each(x => x.Parent = this);
        }

        public string Title { get; set; }

        internal void Execute(IScenarioWriter writer)
        {
            InMemoryQueueManager.ClearAll();
            TestMessageRecorder.Clear();
            MessageHistory.ClearAll();

            _configurations.Each(x => x.SpinUp());

            writer.WriteTitle(Title);

            using (writer.Indent())
            {
                writeArrangement(writer);

                _steps.Each(x => x.Act(writer));
                writer.BlankLine();

                _steps.Each(x => x.Assert(writer));

                // TODO -- blow up if there are unexpected messages
            }
        }

        internal void Preview(IScenarioWriter writer)
        {
            writer.WriteTitle(Title);

            using (writer.Indent())
            {
                writeArrangement(writer);

                _steps.Each(x => x.PreviewAct(writer));
                writer.BlankLine();

                _steps.Each(x => x.PreviewAssert(writer));
            }
        }

        private void writeArrangement(IScenarioWriter writer)
        {
            _configurations.Each(x => {
                x.Describe(writer);
                writer.BlankLine();
            });

            writer.BlankLine();
        }

        public SendExpression<T> Send<T>(string description) where T : Message, new()
        {
            return new SendExpression<T>(this, description);
        } 

        public class SendExpression<T> where T : Message, new()
        {
            public SendExpression(Scenario parent, string description)
            {
            }

            public SendExpression<T> ShouldBeReceivedBy(NodeConfiguration node)
            {

                return this;
            }

            public SendExpression<T> MatchingMessageIsReceivedBy<T1>(NodeConfiguration nodeConfiguration)
            {
                throw new System.NotImplementedException();
            }
        }

        public GivenRequestExpression<TRequest> GivenRequest<TRequest>(string description) where TRequest : Message, new()
        {
            return new GivenRequestExpression<TRequest>(this, description);
        }

        // TODO -- clean up the FI mechanics to make it lead the user.
        public class GivenRequestExpression<TRequest> where TRequest : Message, new()
        {
            public GivenRequestExpression(Scenario parent, string description)
            {
                throw new System.NotImplementedException();
            }

            public GivenRequestExpression<TRequest> From(NodeConfiguration nodeConfiguration)
            {
                throw new System.NotImplementedException();
            }

            public GivenRequestExpression<TRequest> SentBy(NodeConfiguration nodeConfiguration)
            {
                throw new System.NotImplementedException();
            }

            public GivenRequestExpression<TRequest> ExpectReply<TReply>() where TReply : Message
            {
                throw new System.NotImplementedException();
            }


        }

        public class RequestExpression<TRequest> where TRequest : Message, new()
        {
            public RequestExpression(Scenario parent, string description)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}

