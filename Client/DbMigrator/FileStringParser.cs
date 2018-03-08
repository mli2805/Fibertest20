using System;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DbMigrator
{
    public class FileStringParser
    {
        private readonly Graph _graph;

        public FileStringParser(Graph graph)
        {
            _graph = graph;
        }

        public void ParseFiber(string[] parts)
        {
            var nodeId1 = int.Parse(parts[1]);
            var nodeId2 = int.Parse(parts[2]);

            var evnt = new AddFiber()
            {
                Id = Guid.NewGuid(),
                Node1 = _graph.NodesDictionary[nodeId1],
                Node2 = _graph.NodesDictionary[nodeId2]
            };
            _graph.Db.Add(evnt);
        }

        public void ParseNode(string[] parts)
        {
            var nodeId = int.Parse(parts[1]);
            var nodeGuid = Guid.NewGuid();
            _graph.NodesDictionary.Add(nodeId, nodeGuid);
            var type = OldEquipmentTypeConvertor(int.Parse(parts[2]));

            if (type == EquipmentType.Rtu)
            {
                var rtuGuid = Guid.NewGuid();
                _graph.Db.Add(new AddRtuAtGpsLocation()
                {
                    Id = rtuGuid,
                    NodeId = nodeGuid,
                    Latitude = double.Parse(parts[3]),
                    Longitude = double.Parse(parts[4]),
                });
                _graph.Db.Add(new UpdateRtu()
                {
                    Id = rtuGuid,
                    Title = parts[5].Trim(),
                    Comment = parts[6].Trim(),
                });
                _graph.NodeToRtuDictionary.Add(nodeGuid, rtuGuid);
            }
            else
            {
                _graph.Db.Add(new AddEquipmentAtGpsLocation()
                {
                    NodeId = nodeGuid,
                    Type = EquipmentType.EmptyNode,
                    // RequestedEquipmentId = Guid.NewGuid(),
                    EmptyNodeEquipmentId = Guid.NewGuid(),
                    Latitude = double.Parse(parts[3]),
                    Longitude = double.Parse(parts[4]),
                });
                _graph.Db.Add(new UpdateNode()
                {
                    Id = nodeGuid,
                    Title = parts[5].Trim(),
                    Comment = parts[6].Trim(),
                });
            }

        }

        public void ParseRtu(string[] parts)
        {
            var nodeId = int.Parse(parts[1]);
            var nodeGuid = _graph.NodesDictionary[nodeId];
            var rtuGuid = _graph.NodeToRtuDictionary[nodeGuid];

            _graph.Db.Add(new InitializeRtu()
            {
                Id = rtuGuid,
                OwnPortCount = int.Parse(parts[2]),
                FullPortCount = int.Parse(parts[2]), // FullPortCount will be increased by OtauAttached event if happened
                Serial = int.Parse(parts[4]).ToString(),
                MainChannel = new NetAddress() { Ip4Address = parts[5], Port = int.Parse(parts[6]) },
                OtauNetAddress = new NetAddress() { Ip4Address = parts[7], Port = int.Parse(parts[8]) },
                ReserveChannel = new NetAddress() { Ip4Address = parts[9], Port = int.Parse(parts[10]) },
            });
        }

        public void ParseEquipments(string[] parts)
        {
            var equipmentId = int.Parse(parts[1]);
            var equipmentGuid = Guid.NewGuid();
            _graph.EquipmentsDictionary.Add(equipmentId, equipmentGuid);

            var nodeId = int.Parse(parts[2]);
            var type = OldEquipmentTypeConvertor(int.Parse(parts[3]));

            var evnt = new AddEquipmentIntoNode()
            {
                Id = equipmentGuid,
                NodeId = _graph.NodesDictionary[nodeId],
                Type = type,
                Title = parts[4].Trim(),
                Comment = parts[5].Trim(),
            };
            _graph.Db.Add(evnt);
        }

        public void ParseCharon(string[] parts)
        {
            var nodeId = int.Parse(parts[2]);
            var nodeGuid = _graph.NodesDictionary[nodeId];
            var rtuGuid = _graph.NodeToRtuDictionary[nodeGuid];

            _graph.Db.Add(new AttachOtau()
            {
                Id = Guid.NewGuid(),
                RtuId = rtuGuid,
                NetAddress = new NetAddress() { Ip4Address = parts[3], Port = int.Parse(parts[4]), IsAddressSetAsIp = true },
                Serial = parts[5],
                PortCount = int.Parse(parts[6]),
                MasterPort = int.Parse(parts[8]),
            });
        }

        private EquipmentType OldEquipmentTypeConvertor(int oldType)
        {
            switch (oldType)
            {
                case 2: return EquipmentType.Closure;
                case 3: return EquipmentType.Cross;
                case 4: return EquipmentType.Rtu;
                case 5: return EquipmentType.Other;
                case 6: return EquipmentType.Terminal;
            }

            throw new Exception();
        }
    }
}