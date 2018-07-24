using System;

namespace Iit.Fibertest.Client
{
    public class CurrentDatacenterParameters
    {
        public string ServerTitle { get; set; }
        public string ServerIp { get; set; }
        public Guid GraphDbVersionId { get; set; }
        public string DatacenterVersion { get; set; }
    }
}