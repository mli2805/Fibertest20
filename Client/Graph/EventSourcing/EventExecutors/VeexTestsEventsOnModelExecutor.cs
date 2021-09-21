using System.Linq;
using AutoMapper;

namespace Iit.Fibertest.Graph
{
    public static class VeexTestsEventsOnModelExecutor
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        public static string AddVeexTest(this Model model, VeexTestAdded e)
        {
            if (model.VeexTests.All(t => t.TestId != e.TestId))
                model.VeexTests.Add(Mapper.Map<VeexTest>(e));
           
            return null;
        }

        public static string RemoveVeexTest(this Model model, VeexTestRemoved e)
        {
            var test = model.VeexTests.FirstOrDefault(t => t.TestId == e.TestId);
            if (test != null)
                model.VeexTests.Remove(test);
            return null;
        }
    }
}