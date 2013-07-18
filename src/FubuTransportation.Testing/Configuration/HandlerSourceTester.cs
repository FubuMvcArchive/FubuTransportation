using System.Diagnostics;
using FubuTransportation.Configuration;
using NUnit.Framework;
using FubuCore;
using System.Linq;
using FubuTestingSupport;
using System.Collections.Generic;

namespace FubuTransportation.Testing.Configuration
{
    [TestFixture]
    public class HandlerSourceTester
    {
        [Test]
        public void apply_to_single_assembly_looking_for_Handler()
        {
            var source = new HandlerSource();
            source.UseThisAssembly();
            source.IncludeClassesSuffixedWithConsumer();

            var descriptions = source.As<IHandlerSource>().FindCalls().Select(x => x.Description);
            descriptions
                .ShouldHaveTheSameElementsAs(
                "MyConsumer.M1(M1 input) : void",
"MyConsumer.M2(M2 input) : void",
"MyConsumer.M3(M3 input) : void",
"MyOtherConsumer.M2(M2 input) : void",
"MyOtherConsumer.M3(M3 input) : void",
"MyOtherConsumer.M4(M4 input) : void");
        }

        [Test]
        public void uses_the_containing_assembly_by_default()
        {
            var source = new HandlerSource();
            source.IncludeClassesSuffixedWithConsumer();

            var descriptions = source.As<IHandlerSource>().FindCalls().Select(x => x.Description);
            descriptions
                .ShouldHaveTheSameElementsAs(
                "MyConsumer.M1(M1 input) : void",
"MyConsumer.M2(M2 input) : void",
"MyConsumer.M3(M3 input) : void",
"MyOtherConsumer.M2(M2 input) : void",
"MyOtherConsumer.M3(M3 input) : void",
"MyOtherConsumer.M4(M4 input) : void");
        }
    }

    public class MyConsumer
    {
        public void M1(M1 input){}
        public void M2(M2 input){}
        public void M3(M3 input){}
    }

    public class MyOtherConsumer
    {
        public void M2(M2 input) { }
        public void M3(M3 input) { }        
        public void M4(M4 input) { }        
    }



    public class M1{}
    public class M2{}
    public class M3{}
    public class M4{}
}