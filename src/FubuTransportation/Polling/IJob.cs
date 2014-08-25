using System.Threading;

namespace FubuTransportation.Polling
{
    public interface IJob
    {
        void Execute(CancellationToken cancellation);
    }
}