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
            e.ExcludedTraceIds = olds.Select(r => r.TraceId).ToList();

            foreach (var oldRelation in olds)
            {
                  model.GponPortRelations.Remove(oldRelation);
            }

            // проверку OtauPortDto убрать потом, это прошлая версия их не заполняла
            foreach (var newRelation in e.AllRelationsOfTce.Where(r => r.OtauPortDto != null))
            {
                if (e.ExcludedTraceIds.Contains(newRelation.TraceId))
                {
                    e.ExcludedTraceIds.Remove(newRelation.TraceId);
                }
                else
                {
                    var trace = model.Traces.FirstOrDefault(t => t.TraceId == newRelation.TraceId);
                    if (trace != null)
                        trace.IsTraceLinkedWithTce = true;
                }

                model.GponPortRelations.Add(newRelation);
            }

            foreach (var excludedTraceId in e.ExcludedTraceIds)
            {
                var trace = model.Traces.FirstOrDefault(t => t.TraceId == excludedTraceId);
                if (trace != null)
                    trace.IsTraceLinkedWithTce = false;
            }

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