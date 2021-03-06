﻿using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public interface IPortOwner
    {
        int OwnPortCount { get; set; }
        ChildrenImpresario ChildrenImpresario { get; }
        NetAddress OtauNetAddress { get; set; }
        string Serial { get; set; }
    }
}