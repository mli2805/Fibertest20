using AutoMapper;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class VeexTestMappingProfile : Profile
    {
        public VeexTestMappingProfile()
        {
            CreateMap<VeexTestCreatedDto, AddVeexTest>();
        }
    }
}