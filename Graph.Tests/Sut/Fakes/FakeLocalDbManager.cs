using System;
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

        public Task SaveEvents(string[] jsons)
        {
            foreach (var json in jsons)
            {
                _localBase.Add(_currentEventNumber, json);
                _currentEventNumber++;
            }
            return Task.FromResult(1);
        }

        public Task<string[]> LoadEvents()
        {
            return Task.FromResult(_localBase.Values.ToArray());
        }

        public void Initialize(string serverAddress, Guid graphDbVersionOnServer)
        {
            
        }
    }
}
