using System;
using System.Collections.Generic;

namespace FubuTransportation.Polling
{
    public interface IPollingJobs : IEnumerable<IPollingJob>
    {
        bool IsActive<T>() where T : IJob;
        bool IsActive(Type jobType);
        void Activate<T>() where T : IJob;
        void Activate(Type type);
    }
}