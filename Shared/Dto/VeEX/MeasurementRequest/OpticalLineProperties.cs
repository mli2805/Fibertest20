using System.Collections.Generic;

// ReSharper disable InconsistentNaming
namespace Iit.Fibertest.Dto
{
    public class OpticalLineProperties
    {
        public string kind { get; set; }
        public List<LasersProperty> lasersProperties { get; set; }
    }
}