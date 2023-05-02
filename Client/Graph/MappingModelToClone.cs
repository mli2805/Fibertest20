using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class MappingModelToClone : Profile
    {
        public MappingModelToClone()
        {
            CreateMap<Node, Node>();
            CreateMap<Fiber, Fiber>();
            CreateMap<Equipment, Equipment>();

            CreateMap<Landmark, Landmark>();

            CreateMap<TraceModelForBaseRef, TraceModelForBaseRef>();
        }
    }

    public static class CloneExt
    {
        private static readonly IMapper Cloner = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingModelToClone>()).CreateMapper();

        public static void CloneInto(this Node source, Node destination)
        {
            Cloner.Map(source, destination);
        }
      
        public static void CloneInto(this Equipment source, Equipment destination)
        {
            Cloner.Map(source, destination);
        }

        public static Landmark Clone(this Landmark source)
        {
            return Cloner.Map<Landmark>(source);
        }

        public static List<Landmark> Clone(this List<Landmark> source)
        {
            return source.Select(l=>Cloner.Map<Landmark>(l)).ToList();
        }

        public static TraceModelForBaseRef Clone(this TraceModelForBaseRef source)
        {
            return Cloner.Map<TraceModelForBaseRef>(source);
        }

        public static void UpdateFrom(this Node destination, Landmark source)
        {
            destination.Title = source.NodeTitle;
            destination.Comment = source.NodeComment;
            destination.Position = source.GpsCoors;
        }

        public static void UpdateFrom(this Equipment destination, Landmark source)
        {
            destination.Title = source.EquipmentTitle;
            destination.Type = source.EquipmentType;
            if (source.EquipmentType == EquipmentType.CableReserve)
                source.RightCableReserve = 0;
            destination.CableReserveLeft = source.LeftCableReserve;
            destination.CableReserveRight = source.RightCableReserve;
        }
    }
}