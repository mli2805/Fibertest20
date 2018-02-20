using AutoMapper;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class UserMappings : Profile
    {
        public UserMappings()
        {
            CreateMap<User, UserVm>();
            CreateMap<UserVm, User>();
        }
    }
}