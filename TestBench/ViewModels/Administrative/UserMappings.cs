using AutoMapper;

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