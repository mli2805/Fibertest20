using AutoMapper;
using Iit.Fibertest.Dto;

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