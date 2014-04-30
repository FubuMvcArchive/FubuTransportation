using System;
using FubuTransportation;

namespace DiagnosticsHarness
{
    public class HarnessSettings
    {
        public HarnessSettings()
        {
            Channel = "memory://harness".ToUri();
            Publisher = "memory://publisher".ToUri();
        }

        public Uri Publisher { get; set; }

        public Uri Channel { get; set; }
    }
}