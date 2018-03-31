using System.Linq;
using AutoMapper;

namespace Iit.Fibertest.Graph
{
    public class ZoneEventsOnModelExecutor
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        private readonly IModel _model;

        public ZoneEventsOnModelExecutor(IModel model)
        {
            _model = model;
        }
        public string AddZone(ZoneAdded e)
        {
            _model.Zones.Add(_mapper.Map<Zone>(e));
            return null;
        }

        public string UpdateZone(ZoneUpdated source)
        {
            var destination =  _model.Zones.First(f => f.ZoneId == source.ZoneId);
            _mapper.Map(source, destination);
            return null;
        }

        public string RemoveZone(ZoneRemoved e)
        {
            _model.Zones.Remove( _model.Zones.First(f => f.ZoneId == e.ZoneId));
            return null;
        }

        public string ChangeResponsibilities(ResponsibilitiesChanged e)
        {
            _model.ChangeResponsibilities(e);
            return null;
        }
    }
}