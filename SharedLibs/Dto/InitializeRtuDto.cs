﻿using System;
using System.Runtime.Serialization;

namespace Dto
{
    [DataContract]
    public class InitializeRtuDto
    {
        [DataMember]
        public Guid RtuId { get; set; }

        [DataMember]
        public string RtuIpAddress { get; set; }

        [DataMember]
        public string DataCenterIpAddress { get; set; }
    }
}
