using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.Graph
{
   
    public class UpdateFromLandmarksBatch : PropertyChangedBase
    {
        public List<UpdateAndMoveNode> Nodes = new List<UpdateAndMoveNode>();
        public List<UpdateFiber> Fibers = new List<UpdateFiber>();
        public List<UpdateEquipment> Equipments = new List<UpdateEquipment>();

        public List<IncludeEquipmentIntoTrace> ReplaceEqs = new List<IncludeEquipmentIntoTrace>();

        public bool Any() => Nodes.Any() || Fibers.Any() || Equipments.Any() || ReplaceEqs.Any();
    }

    public static class UpdateFromLandmarksViewExt
    {
        private static IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingModelToCmdProfile>()).CreateMapper();

        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        public static LandmarksCorrectionDto BuildDto(this UpdateFromLandmarksBatch command)
        {
            if (!command.Any()) return null;
            var dto = new LandmarksCorrectionDto() { BatchId = Guid.NewGuid() };
            dto.Corrections.AddRange(command.Nodes
                .Select(o=>JsonConvert.SerializeObject(o, JsonSerializerSettings)));
            dto.Corrections.AddRange(command.Fibers
                .Select(o=>JsonConvert.SerializeObject(o, JsonSerializerSettings)));
            dto.Corrections.AddRange(command.Equipments
                .Select(o=>JsonConvert.SerializeObject(o, JsonSerializerSettings)));
            dto.Corrections.AddRange(command.ReplaceEqs
                .Select(o=>JsonConvert.SerializeObject(o, JsonSerializerSettings)));
            return dto;
        }

        public static void ClearAll(this UpdateFromLandmarksBatch command)
        {
            command.Nodes.Clear();
            command.Fibers.Clear();
            command.Equipments.Clear();
            command.ReplaceEqs.Clear();
        }

        public static void ClearNodeCommands(this UpdateFromLandmarksBatch command, Guid nodeId)
        {
            command.Nodes.RemoveAll(i => i.NodeId == nodeId);
        }

        public static void Add(this UpdateFromLandmarksBatch command, Node node)
        {
            var item = _mapper.Map<UpdateAndMoveNode>(node);
            command.Nodes.RemoveAll(i => i.NodeId == node.NodeId); // could not be more than one
            command.Nodes.Add(item);
        }

        public static void ClearFiberCommands(this UpdateFromLandmarksBatch command, Guid fiberId)
        {
            command.Fibers.RemoveAll(i => i.Id == fiberId);
        }

        public static void Add(this UpdateFromLandmarksBatch command, Fiber fiber)
        {
            var item = new UpdateFiber() { Id = fiber.FiberId, UserInputedLength = (int)fiber.UserInputedLength };
            command.Fibers.RemoveAll(i => i.Id == fiber.FiberId); // could not be more than one
            command.Fibers.Add(item);
        }

        public static void ClearEquipmentCommands(this UpdateFromLandmarksBatch command, Guid equipmentId)
        {
            command.Equipments.RemoveAll(i => i.EquipmentId == equipmentId);
        }

        public static void Add(this UpdateFromLandmarksBatch command, Equipment equipment)
        {
            var item = _mapper.Map<UpdateEquipment>(equipment);
            // could not be more than one
            command.Equipments.RemoveAll(i => i.EquipmentId == equipment.EquipmentId);
            command.Equipments.Add(item);
        }

        public static void ClearReplaceCommands(this UpdateFromLandmarksBatch command, int numberWithAdjustmentPoints)
        {
            command.ReplaceEqs.RemoveAll(i => i.IndexInTrace == numberWithAdjustmentPoints);
        }

        public static void Add(this UpdateFromLandmarksBatch command, IncludeEquipmentIntoTrace sub)
        {
            command.ReplaceEqs.Add(sub);
        }
    }
}
