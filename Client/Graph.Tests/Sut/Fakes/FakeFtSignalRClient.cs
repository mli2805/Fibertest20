using System.Threading.Tasks;
using Iit.Fibertest.DataCenterCore;

namespace Graph.Tests
{
    public class FakeFtSignalRClient : IFtSignalRClient
    {
        public Task<bool> IsSignalRConnected(bool isLog = true)
        {
            return Task.FromResult(true);
        }

        public Task NotifyAll(string eventType, string dataInJson)
        {
            return Task.Delay(1);
        }

        public Task<bool> CheckServerIn()
        {
            return Task.FromResult(true);
        }
    }
}