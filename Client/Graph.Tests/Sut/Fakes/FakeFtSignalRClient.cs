using System.Threading.Tasks;
using Iit.Fibertest.DataCenterCore;

namespace Graph.Tests
{
    public class FakeFtSignalRClient : IFtSignalRClient
    {
        public Task NotifyAll(string eventType, string dataInJson)
        {
            return Task.Delay(1);
        }
    }
}