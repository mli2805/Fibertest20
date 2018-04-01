using System.Linq;
using AutoMapper;

namespace Iit.Fibertest.Graph
{
    public class UserEventsOnModelExecutor
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();
        private readonly Model _model;

        public UserEventsOnModelExecutor(Model model)
        {
            _model = model;
        }
        public string AddUser(UserAdded e)
        {
            _model.Users.Add(Mapper.Map<User>(e));
            return null;
        }

        public string UpdateUser(UserUpdated source)
        {
            var destination =  _model.Users.First(f => f.UserId == source.UserId);
            Mapper.Map(source, destination);
            return null;
        }

        public string RemoveUser(UserRemoved e)
        {
            _model.Users.Remove( _model.Users.First(f => f.UserId == e.UserId));
            return null;
        }
    }
}