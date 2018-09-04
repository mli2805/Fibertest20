using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace DbMigrationWpf
{
    public class Charon15
    {
        public int RtuId;
        public NetAddress OtauAddress;
        public int FirstPortNumber;
        public int PortCount;
    }
    public class GraphModel
    {
        public readonly List<object> Commands = new List<object>();

        public readonly Dictionary<int, Guid> NodesDictionary = new Dictionary<int, Guid>();
        public readonly Dictionary<Guid, Guid> NodeToRtuDictionary = new Dictionary<Guid, Guid>();
        public readonly Dictionary<int, Guid> EquipmentsDictionary = new Dictionary<int, Guid>();
        public readonly Dictionary<int, Guid> TracesDictionary = new Dictionary<int, Guid>();

        public readonly List<InitializeRtu> RtuCommands = new List<InitializeRtu>();
        public readonly List<Charon15> Charon15S = new List<Charon15>();
        public readonly Dictionary<Guid, Guid> EmptyNodes = new Dictionary<Guid, Guid>(); // nodeGuid ; emptyNodeEquipmentGuid
        public readonly List<object> TraceEventsUnderConstruction = new List<object>();

        public readonly List<AddTrace> AddTraceCommands = new List<AddTrace>();
        public readonly List<AttachTrace> AttachTraceCommands = new List<AttachTrace>();
    }
}