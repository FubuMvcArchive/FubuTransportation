using System;

namespace FubuTransportation.Testing.TestSupport
{
    public interface IScenarioWriter
    {
        IDisposable Indent();

        void WriteLine(string format, params object[] parameters);
    }
}