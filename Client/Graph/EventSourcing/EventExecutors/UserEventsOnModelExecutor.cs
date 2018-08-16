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

        public string ApplyLicense(LicenseApplied e)
        {
            if (_model.License == null)
                _model.License = new License();
            _model.License.LicenseIds.Add(e.LicenseId);

            if (e.Owner != "")
                _model.License.Owner = e.Owner;
            if (e.RtuCount.Value != -1)
                _model.License.RtuCount = e.RtuCount;
            if (e.ClientStationCount.Value != -1)
                _model.License.ClientStationCount = e.ClientStationCount;
            if (e.SuperClientStationCount.Value != -1)
                _model.License.SuperClientStationCount = e.SuperClientStationCount;
            return null;
        }

        public string UpdateUser(UserUpdated source)
        {
            var destination = _model.Users.First(f => f.UserId == source.UserId);
            Mapper.Map(source, destination);
            return null;
        }

        public string RemoveUser(UserRemoved e)
        {
            _model.Users.Remove(_model.Users.First(f => f.UserId == e.UserId));
            return null;
        }
    }
}