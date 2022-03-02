using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class ModelFromFileExporter
    {
        private readonly Model _readModel;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;

        public ModelFromFileExporter(Model readModel, IWcfServiceDesktopC2D c2DWcfManager)
        {
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
        }

        public async Task<string> Apply(Model oneRtuModel)
        {
            var rtu = oneRtuModel.Rtus.First();
            var rtuNode = oneRtuModel.Nodes.First(n => n.NodeId == rtu.NodeId);
            var cmd = new AddRtuAtGpsLocation()
            {
                Id = rtu.Id,
                NodeId = rtu.NodeId,
                Title = rtu.Title,
                Latitude = rtuNode.Position.Lat,
                Longitude = rtuNode.Position.Lng
            };
            var res = await _c2DWcfManager.SendCommandAsObj(cmd);
            await Task.Delay(2000);
            var initializeRtu = new InitializeRtu()
            {
                Id = rtu.Id,
                Maker = RtuMaker.IIT,
                FullPortCount = rtu.FullPortCount,
                OwnPortCount = rtu.OwnPortCount,
                Serial = rtu.Serial,
                OtauNetAddress = rtu.OtdrNetAddress
            };
            var unused2 = await _c2DWcfManager.SendCommandAsObj(initializeRtu);

            foreach (var otau in oneRtuModel.Otaus)
            {
                var attachOtau = new AttachOtau()
                {
                    Id = otau.Id,
                    RtuId = rtu.Id,
                    MasterPort = otau.MasterPort,
                    PortCount = otau.PortCount,
                    NetAddress = otau.NetAddress,
                    Serial = otau.Serial,
                    IsOk = otau.IsOk
                };
                var unused = await _c2DWcfManager.SendCommandAsObj(attachOtau);
            }

            var commandList = new List<object>();

            foreach (var node in oneRtuModel.Nodes)
            {
                if (_readModel.Nodes.Any(n => n.NodeId == node.NodeId)) continue;

                commandList.Clear();
                var addEquipmentAtGpsLocation = new AddEquipmentAtGpsLocation()
                {
                    NodeId = node.NodeId,
                    EmptyNodeEquipmentId = Guid.Empty,
                    RequestedEquipmentId = Guid.Empty,
                    Latitude = node.Position.Lat,
                    Longitude = node.Position.Lng,
                    Type = node.TypeOfLastAddedEquipment,
                };
                commandList.Add(addEquipmentAtGpsLocation);

                var updateNode = new UpdateNode() { NodeId = node.NodeId, Title = node.Title };
                commandList.Add(updateNode);
                var _ = await _c2DWcfManager.SendCommandsAsObjs(commandList);
            }

            foreach (var equipment in oneRtuModel.Equipments)
            {
                if (_readModel.Equipments.Any(n => n.EquipmentId == equipment.EquipmentId))
                    continue;

                commandList.Clear();
                var eqNode = _readModel.Nodes.First(n => n.NodeId == equipment.NodeId);
                var addEquipmentIntoNode = new AddEquipmentIntoNode()
                {
                    EquipmentId = equipment.EquipmentId,
                    Title = equipment.Title,
                    NodeId = eqNode.NodeId,
                    Type = equipment.Type
                };
                commandList.Add(addEquipmentIntoNode);
                var _ = await _c2DWcfManager.SendCommandsAsObjs(commandList);
            }

            foreach (var fiber in oneRtuModel.Fibers)
            {
                if (_readModel.Fibers.Any(n => n.FiberId == fiber.FiberId)) continue;

                commandList.Clear();
                var addFiber = new AddFiber()
                {
                    FiberId = fiber.FiberId,
                    NodeId1 = fiber.NodeId1,
                    NodeId2 = fiber.NodeId2
                };
                commandList.Add(addFiber);
                var _ = await _c2DWcfManager.SendCommandsAsObjs(commandList);
            }

            foreach (var trace in oneRtuModel.Traces)
            {
                commandList.Clear();
                var addTrace = new AddTrace()
                {
                    TraceId = trace.TraceId,
                    RtuId = trace.RtuId,
                    Title = trace.Title,
                    Comment = trace.Comment,
                    NodeIds = trace.NodeIds,
                    EquipmentIds = trace.EquipmentIds,
                    FiberIds = trace.FiberIds,
                };
                commandList.Add(addTrace);

                if (trace.IsAttached)
                {
                    var attachTrace = new AttachTrace() { TraceId = trace.TraceId, OtauPortDto = trace.OtauPort };
                    commandList.Add(attachTrace);
                }
                var _ = await _c2DWcfManager.SendCommandsAsObjs(commandList);
            }

            return res;
        }
    }
}