using System.Collections.Generic;
using System.Linq.Expressions;
using FubuMVC.Core.Registration;

namespace FubuTransportation.Polling
{
    [ApplicationLevel]
    public class PollingJobSettings
    {
        public readonly IList<PollingJobDefinition> Jobs = new List<PollingJobDefinition>();
    }
}