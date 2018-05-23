using System.Linq;
using AutoMapper;

namespace Iit.Fibertest.Graph
{
    public class ZoneEventsOnModelExecutor
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        private readonly Model _model;

        public ZoneEventsOnModelExecutor(Model model)
        {
            _model = model;
        }
        public string AddZone(ZoneAdded e)
        {
            _model.Zones.Add(Mapper.Map<Zone>(e));
            return null;
        }

        public string UpdateZone(ZoneUpdated source)
        {
            var destination =  _model.Zones.First(f => f.ZoneId == source.ZoneId);
            Mapper.Map(source, destination);
            return null;
        }

        public string RemoveZone(ZoneRemoved e)
        {
            foreach (var trace in _model.Traces)
            {
                if (trace.ZoneIds.Contains(e.ZoneId))
                    trace.ZoneIds.Remove(e.ZoneId);
            }

            foreach (var rtu in _model.Rtus)
            {
                if (rtu.ZoneIds.Contains(e.ZoneId))
                    rtu.ZoneIds.Remove(e.ZoneId);
            }

            foreach (var user in _model.Users.Where(u=>u.ZoneId == e.ZoneId).ToList())
                _model.Users.Remove(user);

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