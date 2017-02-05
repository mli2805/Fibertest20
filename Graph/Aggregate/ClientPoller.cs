using System.Collections.Generic;
using System.Linq;
using PrivateReflectionUsingDynamic;

namespace Iit.Fibertest.Graph
{
    public class ClientPoller
    {
        private readonly Db _db;
        public List<object> ReadModels { get; }
        public int CurrentEventNumber { get; private set; }

        public ClientPoller(Db db, List<object> readModels)
        {
            _db = db;
            ReadModels = readModels;
        }

        public void Tick()
        {
            foreach (var e in _db.Events.Skip(CurrentEventNumber))
                foreach (var m in ReadModels)
                    m.AsDynamic().Apply(e);
            CurrentEventNumber = _db.Events.Count;
        }
    }
}