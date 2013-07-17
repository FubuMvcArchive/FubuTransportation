using FubuMVC.Core.Registration.Nodes;

namespace FubuTransportation.Configuration
{
    public class HandlerChain : BehaviorChain
    {
        public static readonly string Category = "Handler";

        public HandlerChain()
        {
            UrlCategory.Category = Category;
            IsPartialOnly = true;
        }

        /*
         * Maybe another node to handle serializers?  Thinking there might be a special one
         * 
         */
    }
}