using System.Linq;
using AutoMapper;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph
{
    public class FiberEventsOnModelExecutor
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();
        private readonly Model _model;
        private readonly IMyLog _logFile;

        public FiberEventsOnModelExecutor(Model model, IMyLog logFile)
        {
            _model = model;
            _logFile = logFile;
        }

        public string AddFiber(FiberAdded e)
        {
            _model.Fibers.Add(Mapper.Map<Fiber>(e));
            return null;
        }

        public string UpdateFiber(FiberUpdated source)
        {
            var destination = _model.Fibers.FirstOrDefault(f => f.FiberId == source.Id);
            if (destination == null)
            {
                var message = $@"FiberUpdated: Fiber {source.Id.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }
            Mapper.Map(source, destination);
            return null;
        }

        public string RemoveFiber(FiberRemoved e)
        {
            var fiber = _model.Fibers.FirstOrDefault(f => f.FiberId == e.FiberId);
            if (fiber == null)
            {
                var message = $@"FiberRemoved: Fiber {e.FiberId.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }
            _model.RemoveFiberUptoRealNodesNotPoints(fiber);
            return null;
        }
    }
}