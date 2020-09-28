using System;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace DbMigrationWpf
{
    public class FileStringParser
    {
        private readonly GraphModel _graphModel;

        public FileStringParser(GraphModel graphModel)
        {
            _graphModel = graphModel;
        }

        public void ParseFiber(string[] parts)
        {
            var fiberId = int.Parse(parts[1]);
            var nodeId1 = int.Parse(parts[2]);
            var nodeId2 = int.Parse(parts[3]);

            var evnt = new AddFiber()
            {
                FiberId = Guid.NewGuid(),
                NodeId1 = _graphModel.NodesDictionary[nodeId1],
                NodeId2 = _graphModel.NodesDictionary[nodeId2]
            };
            _graphModel.Commands.Add(evnt);

            _graphModel.FibersDictionary.Add(fiberId, evnt.FiberId);
        }

        public void ParseNode(string[] parts)
        {
            var nodeId = int.Parse(parts[1]);
            var nodeGuid = Guid.NewGuid();
            _graphModel.NodesDictionary.Add(nodeId, nodeGuid);
            var type = OldEquipmentTypeConvertor(int.Parse(parts[2]));

            if (type == EquipmentType.Rtu)
            {
                var rtuGuid = Guid.NewGuid();
                _graphModel.Commands.Add(new AddRtuAtGpsLocation()
                {
                    Id = rtuGuid,
                    NodeId = nodeGuid,
                    Latitude = double.Parse(parts[3]),
                    Longitude = double.Parse(parts[4]),
                    Title = parts[5].Trim(),
                    Comment = parts[6].Trim(),
                });
                _graphModel.NodeToRtuDictionary.Add(nodeGuid, rtuGuid);
            }
            else
            {
                var emptyNodeEquipmentGuid = Guid.NewGuid();
                _graphModel.Commands.Add(new AddEquipmentAtGpsLocationWithNodeTitle()
                {
                    NodeId = nodeGuid,
                    Type = EquipmentType.EmptyNode,
                    RequestedEquipmentId = emptyNodeEquipmentGuid,
                    EmptyNodeEquipmentId = Guid.Empty,
                    Latitude = double.Parse(parts[3]),
                    Longitude = double.Parse(parts[4]),
                    Title = parts[5].Trim(),
                    Comment = parts[6].Trim(),
                });
                _graphModel.EmptyNodes.Add(nodeGuid, emptyNodeEquipmentGuid);
            }

        }

        public void ParseRtu(string[] parts)
        {
            var nodeId = int.Parse(parts[1]);
            var nodeGuid = _graphModel.NodesDictionary[nodeId];
            var rtuGuid = _graphModel.NodeToRtuDictionary[nodeGuid];

            var initializeRtu = new InitializeRtu()
            {
                Id = rtuGuid,
                OwnPortCount = int.Parse(parts[2]),
                FullPortCount = int.Parse(parts[2]), // FullPortCount will be increased by OtauAttached event if happened
             //   Serial = int.Parse(parts[4]).ToString(),
                Serial = parts[4].Trim(),
                MainChannel = new NetAddress() { Ip4Address = parts[5].Trim(), Port = int.Parse(parts[6]) == 11832 ? 11842 : int.Parse(parts[6]) },
                OtauNetAddress = new NetAddress()
                {
                    Ip4Address = parts[5].Trim() == parts[7].Trim() ? "192.168.88.101" : parts[7].Trim(),
                    Port = 23 // main charon always 23
                },
                ReserveChannel = new NetAddress() { Ip4Address = parts[9].Trim(), Port = int.Parse(parts[10]) },
                Version = "",
                AcceptableMeasParams = new TreeOfAcceptableMeasParams(),
            };

            _graphModel.Commands.Add(initializeRtu);
            _graphModel.RtuCommands.Add(initializeRtu);
        }

        public void ParseEquipments(string[] parts)
        {
            var equipmentId = int.Parse(parts[1]);
            var equipmentGuid = Guid.NewGuid();
            _graphModel.EquipmentsDictionary.Add(equipmentId, equipmentGuid);

            var nodeId = int.Parse(parts[2]);
            if (nodeId == 0) return;
            var type = OldEquipmentTypeConvertor(int.Parse(parts[3]));

            var evnt = new AddEquipmentIntoNode()
            {
                EquipmentId = equipmentGuid,
                NodeId = _graphModel.NodesDictionary[nodeId],
                Type = type,
                Title = parts[4].Trim(),
                Comment = parts[5].Trim(),
            };
            _graphModel.Commands.Add(evnt);
        }

        public void ParseCharon(string[] parts)
        {
            var nodeId = int.Parse(parts[2]);
            var nodeGuid = _graphModel.NodesDictionary[nodeId];
            var rtuGuid = _graphModel.NodeToRtuDictionary[nodeGuid];

            var charonAddress = new NetAddress() { Ip4Address = parts[3].Trim(), Port = int.Parse(parts[4]), IsAddressSetAsIp = true };
            var attachOtau = new AttachOtau()
            {
                Id = Guid.NewGuid(),
                RtuId = rtuGuid,
                NetAddress = charonAddress,
                Serial = parts[5].Trim(),
                PortCount = int.Parse(parts[6]),
                MasterPort = int.Parse(parts[8]),
            };
            _graphModel.Commands.Add(attachOtau);
            _graphModel.Charon15S.Add(new Charon15()
            {
                RtuId = nodeId,
                NetAddress = charonAddress,
                Serial = parts[5].Trim(),
                FirstPortNumber = int.Parse(parts[7]),
                PortCount = int.Parse(parts[6]),
            });
        }

        private EquipmentType OldEquipmentTypeConvertor(int oldType)
        {
            switch (oldType)
            {
                case 0: return EquipmentType.Other; // несколько оборудований в мгтс

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