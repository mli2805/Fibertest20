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

        public static string UpdateAllTceGponRelations(this Model model, AllTceGponRelationsUpdated e)
        {
            var olds = model.GponPortRelations.Where(r => r.TceId == e.TceId).ToList();
            foreach (var oldRelation in olds)
            {
                model.GponPortRelations.Remove(oldRelation);
            }

            model.GponPortRelations.AddRange(e.AllTceRelations);
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