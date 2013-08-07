using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using FubuTransportation.Testing.Scenarios;
using FubuTransportation.Testing.TestSupport;
using NUnit.Framework;
using System.Linq;
using FubuCore;
using FubuTestingSupport;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class ScenarioRunner
    {
        [Test, Explicit]
        public void write_previews()
        {
            var scenarios = FindScenarios();
            var writer = new ScenarioWriter();

            scenarios.Each(x => {
                x.Preview(writer);
                writer.BlankLine();
                writer.BlankLine();
            });
        }

        [Test, Explicit]
        public void try_one()
        {
            var writer = new ScenarioWriter();

            new Send_a_single_message_to_the_correct_node().Execute(writer);
            writer.FailureCount.ShouldEqual(0);
        }



        public static IEnumerable<Scenario> FindScenarios()
        {
            return Assembly.GetExecutingAssembly()
                           .GetTypes()
                           .Where(x => x.IsConcreteTypeOf<Scenario>() && x != typeof (Scenario))
                           .Select(x => {
                               return typeof (Builder<>).CloseAndBuildAs<IScenarioBuilder>(x).Build();
                           });

        }

        public interface IScenarioBuilder
        {
            Scenario Build();
        }

        public class Builder<T> : IScenarioBuilder where T : Scenario, new()
        {
            public Scenario Build()
            {
                return new T();
            }
        }
    }
}