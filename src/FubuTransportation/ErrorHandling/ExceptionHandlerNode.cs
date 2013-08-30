using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Registration.ObjectGraph;
using FubuTransportation.Configuration;

namespace FubuTransportation.ErrorHandling
{
    public class ExceptionHandlerNode : BehaviorNode
    {
        private readonly HandlerChain _chain;

        public ExceptionHandlerNode(HandlerChain chain)
        {
            _chain = chain;
        }

        protected override ObjectDef buildObjectDef()
        {
            var def = ObjectDef.ForType<ExceptionHandlerBehavior>();
            def.DependencyByValue(_chain);

            return def;
        }

        public override BehaviorCategory Category
        {
            get { return BehaviorCategory.Wrapper; }
        }
    }
}