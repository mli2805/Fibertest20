using System.Threading.Tasks;
using Iit.Fibertest.DataCenterCore;

namespace Graph.Tests
{
    public class FakeFtSignalRClient : IFtSignalRClient
    {
        public Task<bool> IsSignalRConnected()
        {

            return Task.FromResult(true);
        }

        public Task NotifyAll(string eventType, string dataInJson)
        {
            return Task.Delay(1);
        }
    }
}