﻿using System;
using Dto;

namespace Iit.Fibertest.Graph
{
    public class AttachOtau
    {
        public Guid Id { get; set; }
        public Guid RtuId { get; set; }

        public NetAddress NetAddress { get; set; } = new NetAddress();
        public RtuPartState NetAddressState { get; set; }
        public string Serial { get; set; }
        public int PortCount { get; set; }

        public int MasterPort { get; set; }
    }
}