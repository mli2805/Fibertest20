using System.Linq;
using AutoMapper;

namespace Iit.Fibertest.Graph
{
    public class ZoneEventsOnModelExecutor
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        private readonly IModel _model;

        public ZoneEventsOnModelExecutor(ReadModel model)
        {
            _model = model;
        }
        public void AddZone(ZoneAdded e)
        {
            _model.Zones.Add(_mapper.Map<Zone>(e));
        }

        public void UpdateZone(ZoneUpdated source)
        {
            var destination =  _model.Zones.First(f => f.ZoneId == source.ZoneId);
            _mapper.Map(source, destination);
        }

        public void RemoveZone(ZoneRemoved e)
        {
            _model.Zones.Remove( _model.Zones.First(f => f.ZoneId == e.ZoneId));
        }
    }
}