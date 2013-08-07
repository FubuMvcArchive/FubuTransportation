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
            Title = GetType().Name.Replace("_", " ");

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
            _configurations.Each(x => x.SpinUp());

            writer.WriteTitle(Title);

            using (writer.Indent())
            {
                writeArrangement(writer);

                writer.WriteLine("Actions");

                using (writer.Indent())
                {
                    _steps.Each(x => x.PreviewAct(writer));
                }

                writer.BlankLine();

                writer.WriteLine("Assertions");

                using (writer.Indent())
                {
                    _steps.Each(x => x.PreviewAssert(writer));
                }
            }
        }

        private void writeArrangement(IScenarioWriter writer)
        {
            _configurations.Each(x => {
                x.Describe(writer);
            });

            writer.BlankLine();
        }

        internal void AddStep(IScenarioStep step)
        {
            _steps.Add(step);
        }
    }
}

