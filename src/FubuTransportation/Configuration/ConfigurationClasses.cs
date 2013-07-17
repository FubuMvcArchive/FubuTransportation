using System;
using System.Collections.Generic;
using FubuMVC.Core;
using FubuCore;
using System.Linq;

namespace FubuTransportation.Configuration
{
    public interface IHandlerSource
    {
        IEnumerable<HandlerCall> FindCalls();
    }

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
}
