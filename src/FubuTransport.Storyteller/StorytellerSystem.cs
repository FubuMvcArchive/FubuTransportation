using FubuMVC.Core;
using Serenity;
using System;

namespace FubuTransport.Storyteller
{
	public class ReplaceWithYourApplicationSource : IApplicationSource
	{
        public FubuApplication BuildApplication()
        {
            throw new NotImplementedException();
        }
	}

	public class StorytellerSystem : FubuMvcSystem<ReplaceWithYourApplicationSource>
	{

	}
}