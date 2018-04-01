using System.Linq;
using AutoMapper;

namespace Iit.Fibertest.Graph
{
    public class UserEventsOnModelExecutor
    {
        private static readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();
        private readonly IModel _model;

        public UserEventsOnModelExecutor(IModel model)
        {
            _model = model;
        }
        public string AddUser(UserAdded e)
        {
            _model.Users.Add(_mapper.Map<User>(e));
            return null;
        }

        public string UpdateUser(UserUpdated source)
        {
            var destination =  _model.Users.First(f => f.UserId == source.UserId);
            _mapper.Map(source, destination);
            return null;
        }

        public string RemoveUser(UserRemoved e)
        {
            _model.Users.Remove( _model.Users.First(f => f.UserId == e.UserId));
            return null;
        }
    }
}