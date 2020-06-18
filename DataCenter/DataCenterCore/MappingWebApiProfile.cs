using AutoMapper;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DataCenterCore
{
    public class MappingWebApiProfile : Profile
    {
        public MappingWebApiProfile()
        {
            CreateMap<PointLatLng, GeoPoint>();
            CreateMap<AccidentLineModel, AccidentLineDto>();
            CreateMap<Measurement, TraceStateDto>()
                .ForMember(dest => dest.RegistrationTimestamp, opt => opt.MapFrom(src => src.EventRegistrationTimestamp))
                .ForMember(dest => dest.Accidents, opt => opt.Ignore());

            CreateMap<UpdateMeasurementDto, UpdateMeasurement>();
            CreateMap<NetworkEvent, NetworkEventDto>()
                .ForMember(dest => dest.EventId, opt => opt.MapFrom(src => src.Ordinal));
        }
    }
}
