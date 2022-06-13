﻿using AutoMapper;

namespace Iit.Fibertest.Graph
{
    public class MappingModelToCmdProfile : Profile
    {
        public MappingModelToCmdProfile()
        {
            CreateMap<User, UpdateUser>();
            CreateMap<User, AssignUsersMachineKey>();
        }
    }
}