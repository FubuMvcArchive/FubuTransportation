using System;
using System.Linq.Expressions;
using FubuCore.Reflection;
using FubuTransportation.Scheduling;

namespace FubuTransportation.Configuration
{
    public abstract class SchedulerMaker<T> : ISettingsAware
    {
        private readonly Expression<Func<T, int>> _expression;
        private readonly ChannelNode _node;

        public SchedulerMaker(Expression<Func<T, int>> expression, ChannelNode node)
        {
            _expression = expression;
            _node = node;
        }

        void ISettingsAware.ApplySettings(object settings)
        {
            int threadCount = (int) ReflectionHelper.GetAccessor(_expression).GetValue(settings);
            _node.Scheduler = buildScheduler(threadCount);
        }

        protected abstract IScheduler buildScheduler(int threadCount);
    }
}