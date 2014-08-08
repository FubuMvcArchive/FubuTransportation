using System.Collections.Generic;
using FubuMVC.Core.Registration;

namespace FubuTransportation.ScheduledJob
{
    [ApplicationLevel]
    public class ScheduledJobSettings
    {
        public readonly IList<ScheduledJobDefinition> Jobs = new List<ScheduledJobDefinition>();
    }
}