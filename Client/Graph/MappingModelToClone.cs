using AutoMapper;

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

        public static Node Clone(this Node source)
        {
            return Cloner.Map<Node>(source);
        }

        public static Fiber Clone(this Fiber source)
        {
            return Cloner.Map<Fiber>(source);
        }

        public static Equipment Clone(this Equipment source)
        {
            return Cloner.Map<Equipment>(source);
        }

        public static Landmark Clone(this Landmark source)
        {
            return Cloner.Map<Landmark>(source);
        }

        public static TraceModelForBaseRef Clone(this TraceModelForBaseRef source)
        {
            return Cloner.Map<TraceModelForBaseRef>(source);
        }
    }
}