using System.Collections.Generic;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.ObjectGraph;
using FubuTransportation.Sagas;

namespace FubuTransportation
{
    [ApplicationLevel]
    public class TransportSettings
    {
        public readonly IList<ISagaStorage> SagaStorageProviders; 

        public TransportSettings()
        {
            SagaStorageProviders = new List<ISagaStorage>();
            DebugEnabled = false;
        }

        public bool DebugEnabled { get; set; }
    }
}