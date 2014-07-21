using System.Collections.Generic;
using FubuTransportation.Polling;

namespace FubuTransportation.LightningQueues
{
    public class PurgeQueuesJob : IJob
    {
        private readonly IPersistentQueues _persistentQueues;

        public PurgeQueuesJob(IPersistentQueues persistentQueues)
        {
            _persistentQueues = persistentQueues;
        }

        public void Execute()
        {
            _persistentQueues.AllQueueManagers.Each(x => x.PurgeOldData());
        }
    }
}