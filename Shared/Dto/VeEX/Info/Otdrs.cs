﻿using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace Iit.Fibertest.Dto
{
    public class Otdrs
    {
        public List<LinkObject> items { get; set; }
        public int offset { get; set; }
        public int total { get; set; }
    }
}