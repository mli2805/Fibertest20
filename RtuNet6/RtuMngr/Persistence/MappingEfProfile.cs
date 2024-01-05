using AutoMapper;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.RtuMngr
{
    public class MappingEfProfile : Profile
    {
        public MappingEfProfile()
        {
            CreateMap<AccidentInSorEf, AccidentInSor>();
            CreateMap<AccidentInSor, AccidentInSorEf>();

            CreateMap<MoniLevelEf, MoniLevel>();
            CreateMap<MoniLevel, MoniLevelEf>();

            CreateMap<MoniResultEf, MoniResult>();
            CreateMap<MoniResult, MoniResultEf>(); // MoniResultEf does not have SorBytes property

            CreateMap<MonitoringPortEf, MonitoringPort>();
            CreateMap<MonitoringPort, MonitoringPortEf>();
        }
    }
}
