using FubuTransportation.InMemory;
using StoryTeller.Engine;

namespace FubuTransportation.Storyteller.Fixtures
{
    
    public class ModelFixture : Fixture
    {
        public static readonly HarnessSettings Settings 
            = InMemoryTransport.ToInMemory<HarnessSettings>();
    }

    public class RunningNode
    {
        
    }
}