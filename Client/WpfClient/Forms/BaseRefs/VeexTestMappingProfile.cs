using AutoMapper;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class VeexTestMappingProfile : Profile
    {
        public VeexTestMappingProfile()
        {
            CreateMap<VeexTestCreatedDto, AddVeexTest>();
        }
    }
}