﻿using System.Collections.Generic;

namespace Iit.Fibertest.Dto
{
    public class Otdrs
    {
        public List<LinkObject> Items { get; set; }
        public int Offset { get; set; }
        public int Total { get; set; }
    }
}