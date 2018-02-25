using System.Linq;
using AutoMapper;

namespace Iit.Fibertest.Graph
{
    public class FiberEventsOnModelExecutor
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();
        private readonly IModel _model;

        public FiberEventsOnModelExecutor(ReadModel model)
        {
            _model = model;
        }
       
        public void AddFiber(FiberAdded e)
        {
            _model.Fibers.Add(_mapper.Map<Fiber>(e));
        }

        public void UpdateFiber(FiberUpdated source)
        {
            var destination = _model.Fibers.First(f => f.Id == source.Id);
            _mapper.Map(source, destination);
        }

        public void RemoveFiber(FiberRemoved e)
        {
            _model.Fibers.Remove(_model.Fibers.First(f => f.Id == e.Id));
        }
    }
}