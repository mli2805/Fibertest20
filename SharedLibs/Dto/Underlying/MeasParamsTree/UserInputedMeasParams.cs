using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Dto
{
    public class UserInputedMeasParams
    {
        public List<Tuple<int, int>> MeasParams { get; set; } = new List<Tuple<int, int>>();
    }
}