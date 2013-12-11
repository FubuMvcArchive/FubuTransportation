using System.Collections.Generic;

namespace FubuTransportation.Subscriptions
{
    public interface ISubscriptionRepository
    {
        IEnumerable<Subscription> PersistRequirements(string name, params Subscription[] requirements);
        IEnumerable<Subscription> LoadSubscriptions(string name);
    }
}