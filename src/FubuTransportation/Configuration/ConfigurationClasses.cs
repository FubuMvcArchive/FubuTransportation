using System;
using System.Reflection;
using FubuMVC.Core;
using FubuCore;
using FubuMVC.Core.Bootstrapping;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Registration.ObjectGraph;
using FubuMVC.Core.Runtime;

namespace FubuTransportation.Configuration
{
    /*
     * NOTES
     * Set the UrlCategory to "FubuTransport" to tell things apart?  Subclass BehaviorChain instead?
     * 
     * 
     * 
     * 
     * 
     */


    /// <summary>
    /// Use to bootstrap a FubuTransportation application that is not co-hosted with a FubuMVC
    /// application
    /// </summary>
    public static class FubuTransport
    {
        public static IContainerFacilityExpression For<T>() where T : FubuTransportRegistry, new()
        {
            var extension = new T();

            return For(extension);
        }

        public static IContainerFacilityExpression For(FubuTransportRegistry extension)
        {
            var registry = new FubuRegistry();
            extension.As<IFubuRegistryExtension>().Configure(registry);
            return FubuApplication.For(registry);
        }

        public static IContainerFacilityExpression For(Action<FubuTransportRegistry> configuration)
        {
            var extension = new FubuTransportRegistry();
            configuration(extension);

            return For(extension);
 
        }

        public static FubuApplication ServiceBus<T>(this FubuApplication application) where T : FubuTransportRegistry, new()
        {
            // TODO -- toss in a fake bottle to get here
            //return application.
        
            throw new NotImplementedException();
        }
    }


    public class FubuTransportRegistry : IFubuRegistryExtension
    {
        void IFubuRegistryExtension.Configure(FubuRegistry registry)
        {
            throw new System.NotImplementedException();
        }
    }

    public class HandlerChain : BehaviorChain
    {
        /*
         * Set the UrlCategory
         * PartialOnly
         * Maybe another node to handle serializers?  Thinking there might be a special one
         * 
         */
    }

    public class HandlerCall : ActionCallBase
    {
        public HandlerCall(Type handlerType, MethodInfo method)
            : base(handlerType, method)
        {
        }

        public HandlerCall()
        {
        }

        public override BehaviorCategory Category
        {
            get { return BehaviorCategory.Call; }
        }

        protected override ObjectDef buildObjectDef()
        {
            /*
             * if one in, zero out, continue to base build ObjectDef()
             * if one in, one out, we need to do the action type where you immediately send something w/ the correlation Id
             */

            return base.buildObjectDef();
        }
    }
}
