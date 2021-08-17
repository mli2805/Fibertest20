using System.Linq;
using AutoMapper;

namespace Iit.Fibertest.Graph
{
    public static class OltEventsOnModelExecutor
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        public static string AddOlt(this Model model, OltAdded e)
        {
            model.Olts.Add(Mapper.Map<Olt>(e));
            return null;
        }

        public static string AddGponPortRelation(this Model model, GponPortRelationAdded e)
        {
            GponPortRelation newRelation = Mapper.Map<GponPortRelation>(e);
            var olt = model.Olts.FirstOrDefault(o => o.Id == newRelation.OltId);
            if (olt == null)
                return @"Olt not found!";

            var relation = olt.Relations.FirstOrDefault(r => r.GponInterface == newRelation.GponInterface);
            if (relation != null)
                olt.Relations.Remove(relation);

            olt.Relations.Add(newRelation);

            return null;
        }
    }
}