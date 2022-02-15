using System.Linq;
using AutoMapper;

namespace Iit.Fibertest.Graph
{
    public static class TceEventsOnModelExecutor
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        public static string AddOrUpdateTce(this Model model, TceAddedOrUpdated e)
        {
            var tce = model.Tces.FirstOrDefault(o => o.Id == e.Id);
            if (tce == null)
                model.Tces.Add(Mapper.Map<Tce>(e));
            else
            {
                tce.Ip = e.Ip;
                tce.TceType = e.TceType;
                tce.Title = e.Title;
                tce.Ip = e.Ip;
                tce.SlotCount = e.SlotCount;
                tce.Slots = e.Slots;
                tce.Comment = e.Comment;
            }
            return null;
        }

        public static string AddGponPortRelation(this Model model, GponPortRelationAdded e)
        {
            GponPortRelation newRelation = Mapper.Map<GponPortRelation>(e);
            var tce = model.Tces.FirstOrDefault(o => o.Id == newRelation.TceId);
            if (tce == null)
                return @"Tce not found!";

            // var relation = olt.Relations.FirstOrDefault(r => r.GponInterface == newRelation.GponInterface);
            // if (relation != null)
            //     olt.Relations.Remove(relation);
            //
            // olt.Relations.Add(newRelation);

            return null;
        }

        public static string RemoveTce(this Model model, TceRemoved e)
        {
            var tce = model.Tces.FirstOrDefault(o => o.Id == e.Id);
            if (tce != null)
                model.Tces.Remove(tce);
            return null;
        }
    }
}