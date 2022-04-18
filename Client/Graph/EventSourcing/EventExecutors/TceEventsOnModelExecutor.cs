using System.Linq;
using AutoMapper;

namespace Iit.Fibertest.Graph
{
    public static class TceEventsOnModelExecutor
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();


        public static string AddOrUpdateTceWithRelations(this Model model, TceWithRelationsAddedOrUpdated e)
        {
            var tce = model.TcesNew.FirstOrDefault(o => o.Id == e.Id);
            if (tce == null)
                model.TcesNew.Add(Mapper.Map<TceS>(e));
            else
            {
                tce.Ip = e.Ip;
                tce.TceTypeStruct = e.TceTypeStruct;
                tce.Title = e.Title;
                tce.Ip = e.Ip;
                tce.Slots = e.Slots;
                tce.Comment = e.Comment;
            }

            var olds = model.GponPortRelations.Where(r => r.TceId == e.Id).ToList();
            foreach (var oldRelation in olds)
            {
                model.GponPortRelations.Remove(oldRelation);
            }

            model.GponPortRelations.AddRange(e.AllRelationsOfTce);
            return null;
        }

        public static string RemoveTce(this Model model, TceRemoved e)
        {
            var tce = model.TcesNew.FirstOrDefault(o => o.Id == e.Id);
            if (tce != null)
                model.TcesNew.Remove(tce);
            return null;
        }

        public static string ReSeedTceTypes(this Model model, TceTypeStructListReSeeded e)
        {
            model.TceTypeStructs = e.TceTypes;
            return null;
        }
    }
}