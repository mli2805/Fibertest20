﻿using AutoMapper;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class MappingViewModelToCommand : Profile
    {
        public MappingViewModelToCommand()
        {
            CreateMap<EquipmentInfoViewModel, AddEquipmentIntoNode>();
            CreateMap<EquipmentInfoViewModel, UpdateEquipment>();
            CreateMap<RtuUpdateViewModel, UpdateRtu>();
        }
    }
}