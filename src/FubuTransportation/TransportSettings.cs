using System;
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
        public readonly IList<Type> SettingTypes = new List<Type>(); 

        public TransportSettings()
        {
            SagaStorageProviders = new List<ISagaStorage>();
            DebugEnabled = false;
            DelayMessagePolling = 5000;
        }

        public bool DebugEnabled { get; set; }
        public double DelayMessagePolling { get; set; }
    }
}