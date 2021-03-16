using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Client;

namespace Graph.Tests
{
    public class FakeLocalDbManager : ILocalDbManager
    {

        private int _currentEventNumber;
        private Dictionary<int, string> _localBase;

        public FakeLocalDbManager()
        {
            _localBase = new Dictionary<int, string>();
            _currentEventNumber = 1;
        }

        public Task SaveEvents(string[] jsons, int eventId)
        {
            foreach (var json in jsons)
            {
                _localBase.Add(_currentEventNumber, json);
                _currentEventNumber++;
            }
            return Task.FromResult(1);
        }

        public Task<CacheParameters> GetCacheParameters()
        {
            return Task.FromResult(new CacheParameters());
        }

        public Task<string[]> LoadEvents(int lastEventInSnapshot)
        {
            return Task.FromResult(_localBase.Values.ToArray());
        }

        public Task<byte[]> LoadSnapshot(int lastEventInSnapshotOnServer)
        {
            return Task.FromResult(new byte[0]);
        }

        public Task<int> RecreateCacheDb()
        {
            return Task.FromResult(1);
        }

        public Task<int> SaveSnapshot(byte[] portion)
        {
            return Task.FromResult(1);

        }

        public void Initialize()
        {
            
        }
    }
}
