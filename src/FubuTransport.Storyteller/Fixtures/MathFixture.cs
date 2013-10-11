using System;
using System.Diagnostics;
using System.Threading;
using StoryTeller.Engine;

namespace FubuTransport.Storyteller.Fixtures
{
	// TODO -- Delete Me!  I'm just a standin
    public class MathFixture : Fixture
    {
        [FormatAs("Adding {x} to {y} should be {returnValue}")]
        public double Adding(double x, double y)
        {
            return x + y;
        }
    }
}