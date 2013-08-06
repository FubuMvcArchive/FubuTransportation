using System;

namespace FubuTransportation
{
    public interface IScenarioWriter
    {
        IDisposable Indent();

        void WriteLine(string format, params object[] parameters);
        void WriteTitle(string title);
        void BlankLine();

        void Exception(Exception ex);
        void Failure(string format, params object[] parameters);
    }

}