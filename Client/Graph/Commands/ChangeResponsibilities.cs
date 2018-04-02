﻿using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    public class ChangeResponsibilities
    {
        // Key - Subject; Value - list of zones where subject's belongings changed
        public Dictionary<Guid, List<Guid>> ResponsibilitiesDictionary { get; set; }
    }
}