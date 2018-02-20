using AutoMapper;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class MappingViewModelToCommand : Profile
    {
        public MappingViewModelToCommand()
        {
            CreateMap<RtuUpdateViewModel, UpdateRtu>();
            CreateMap<ZoneViewModel, AddZone>();
            CreateMap<ZoneViewModel, UpdateZone>();
        }
    }
}