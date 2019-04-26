using System.Linq;
using AutoMapper;

namespace Iit.Fibertest.Graph
{
    public static class UserEventsOnModelExecutor
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();
        
        public static string AddUser(this Model model, UserAdded e)
        {
            model.Users.Add(Mapper.Map<User>(e));
            return null;
        }

        public static string ApplyLicense(this Model model, LicenseApplied e)
        {
            if (model.License == null)
                model.License = new License();
            model.License.LicenseIds.Add(e.LicenseId);

            if (e.Owner != "")
                model.License.Owner = e.Owner;
            if (e.RtuCount.Value != -1)
                model.License.RtuCount = e.RtuCount;
            if (e.ClientStationCount.Value != -1)
                model.License.ClientStationCount = e.ClientStationCount;
            if (e.SuperClientStationCount.Value != -1)
                model.License.SuperClientStationCount = e.SuperClientStationCount;
            return null;
        }

        public static string UpdateUser(this Model model, UserUpdated source)
        {
            var destination = model.Users.First(f => f.UserId == source.UserId);
            Mapper.Map(source, destination);
            return null;
        }

        public static string RemoveUser(this Model model, UserRemoved e)
        {
            model.Users.Remove(model.Users.First(f => f.UserId == e.UserId));
            return null;
        }
    }
}