using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Bottles;
using FubuCore;
using FubuMVC.Core;

namespace FubuTransportation.Configuration
{
    public class FubuTransportRegistry : IFubuRegistryExtension
    {
        private readonly IList<IHandlerSource> _sources = new List<IHandlerSource>(); 

        // TODO -- need to do something to keep this from getting picked up automatically.

        private IEnumerable<IHandlerSource> allSources()
        {
            if (_sources.Any())
            {
                foreach (var handlerSource in _sources)
                {
                    yield return handlerSource;
                }
            }
            else
            {
                var source = new HandlerSource();
                source.UseThisAssembly();
                source.IncludeClassesSuffixedWithConsumer();

                yield return source;
            }
        } 

        void IFubuRegistryExtension.Configure(FubuRegistry registry)
        {
            var graph = new HandlerGraph();
            var allCalls = allSources().SelectMany(x => x.FindCalls());
            graph.Add(allCalls);

            // TODO -- apply policies of some sort

            registry.AlterSettings<HandlerGraph>(x => x.Import(graph));
        }

        /// <summary>
        ///   Finds the currently executing assembly.
        /// </summary>
        /// <returns></returns>
        public static Assembly FindTheCallingAssembly()
        {
            var trace = new StackTrace(false);

            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            Assembly fubuCore = typeof(ITypeResolver).Assembly;
            Assembly bottles = typeof(IPackageLoader).Assembly;
            Assembly fubumvc = typeof (FubuRegistry).Assembly;
            

            Assembly callingAssembly = null;
            for (int i = 0; i < trace.FrameCount; i++)
            {
                StackFrame frame = trace.GetFrame(i);
                Assembly assembly = frame.GetMethod().DeclaringType.Assembly;
                if (assembly != thisAssembly && assembly != fubuCore && assembly != bottles && assembly != fubumvc && !assembly.GetName().Name.StartsWith("System."))
                {
                    callingAssembly = assembly;
                    break;
                }
            }
            return callingAssembly;
        }
    }
}