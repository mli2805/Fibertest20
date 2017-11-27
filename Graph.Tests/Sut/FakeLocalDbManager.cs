using System.Collections.Generic;
using System.Linq;
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

        public void SaveEvents(string[] jsons)
        {
            foreach (var json in jsons)
            {
                _localBase.Add(_currentEventNumber, json);
                _currentEventNumber++;
            }
        }

        public string[] LoadEvents()
        {
            return _localBase.Values.ToArray();
        }
    }
}
