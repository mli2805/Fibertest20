﻿using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace Iit.Fibertest.Dto
{
    // VeexOtauCascadingScheme myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Connection
    {
        public string outputOtauId { get; set; }
        public int outputOtauPort { get; set; }
        public string inputOtauId { get; set; }
        public int inputOtauPort { get; set; }
    }

    public class RootConnection
    {
        public string inputOtauId { get; set; }
        public int inputOtauPort { get; set; }
    }
    public class VeexOtauCascadingScheme
    {
        public List<Connection> connections { get; set; }
        public List<RootConnection> rootConnections { get; set; }
    }

}
