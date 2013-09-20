using System;
using FubuTransportation;

namespace DiagnosticsHarness
{
    public class HarnessSettings
    {
        public HarnessSettings()
        {
            Channel = "memory://harness".ToUri();
        }

        public Uri Channel { get; set; }
    }
}