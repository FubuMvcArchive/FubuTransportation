using FubuMVC.Core;

namespace DiagnosticsHarness
{
    public class FubuHarnessRegistry : FubuRegistry
    {
        public FubuHarnessRegistry()
        {
            Import<HarnessRegistry>();
        }
    }
}