using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Dto
{
    public class SelectedMeasParams
    {
        public List<Tuple<ServiceFunctionFirstParam, int>> MeasParams { get; set; } = new List<Tuple<ServiceFunctionFirstParam, int>>();
    }
}