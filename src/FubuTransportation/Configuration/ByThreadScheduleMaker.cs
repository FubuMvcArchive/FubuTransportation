using System;
using System.Linq.Expressions;
using FubuTransportation.Scheduling;

namespace FubuTransportation.Configuration
{
    public class ByThreadScheduleMaker<T> : SchedulerMaker<T>
    {
        public ByThreadScheduleMaker(Expression<Func<T, int>> expression, ChannelNode node) : base(expression, node)
        {
        }

        protected override IScheduler buildScheduler(int threadCount)
        {
            return new ThreadScheduler(threadCount);
        }
    }
}