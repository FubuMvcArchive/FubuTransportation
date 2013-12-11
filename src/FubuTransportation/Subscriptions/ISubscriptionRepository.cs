using System.Collections.Generic;

namespace FubuTransportation.Subscriptions
{
    public interface ISubscriptionRepository
    {
        IEnumerable<Subscription> PersistRequirements(params Subscription[] requirements);
        IEnumerable<Subscription> LoadSubscriptions();
    }
}