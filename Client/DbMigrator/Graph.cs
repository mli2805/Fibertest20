using System;
using System.Collections.Generic;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DbMigrator
{
    public class Graph
    {
        public readonly List<object> Commands = new List<object>();

        public readonly Dictionary<int, Guid> NodesDictionary = new Dictionary<int, Guid>();
        public readonly Dictionary<Guid, Guid> NodeToRtuDictionary = new Dictionary<Guid, Guid>();
        public readonly Dictionary<int, Guid> EquipmentsDictionary = new Dictionary<int, Guid>();
        public readonly Dictionary<int, Guid> TracesDictionary = new Dictionary<int, Guid>();

        public readonly List<InitializeRtu> RtuCommands = new List<InitializeRtu>();
        public readonly Dictionary<Guid, int> TracePorts = new Dictionary<Guid, int>();
        public readonly List<object> TraceEventsUnderConstruction = new List<object>();
    }
}