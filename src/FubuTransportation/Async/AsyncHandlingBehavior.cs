﻿using FubuMVC.Core.Behaviors;

namespace FubuTransportation.Async
{
    public class AsyncHandlingBehavior : BasicBehavior
    {
        private readonly IAsyncHandling _asyncHandling;

        public AsyncHandlingBehavior(IAsyncHandling asyncHandling)
            : base(PartialBehavior.Executes)
        {
            _asyncHandling = asyncHandling;
        }

        protected override void afterInsideBehavior()
        {
            _asyncHandling.WaitForAll();
        }
    }
}