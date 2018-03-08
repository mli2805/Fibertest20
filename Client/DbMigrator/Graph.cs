using System;
using System.Collections.Generic;

namespace Iit.Fibertest.DbMigrator
{
    public class Graph
    {
        public readonly List<object> Db = new List<object>();
        public readonly Dictionary<int, Guid> NodesDictionary = new Dictionary<int, Guid>();
        public readonly Dictionary<Guid, Guid> NodeToRtuDictionary = new Dictionary<Guid, Guid>();
        public readonly Dictionary<int, Guid> EquipmentsDictionary = new Dictionary<int, Guid>();
        public readonly Dictionary<int, Guid> TracesDictionary = new Dictionary<int, Guid>();

        public readonly List<object> TraceEventsUnderConstruction = new List<object>();
    }
}