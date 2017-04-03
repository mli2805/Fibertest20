using AutoMapper;

namespace Iit.Fibertest.TestBench
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