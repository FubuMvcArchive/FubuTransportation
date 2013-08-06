using System;

namespace FubuTransportation
{
    public interface IScenarioWriter
    {
        IDisposable Indent();

        void WriteLine(string format, params object[] parameters);
    }
}