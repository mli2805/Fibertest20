﻿using System;
using System.Runtime.Serialization;

namespace Dto
{
        [DataContract]
    public class LastSuccessfullMeasTimeDto
    {
        public bool IsMonitoringOn { get; set; }
        public DateTime LastSuccessfullMeasTimeStamp { get; set; }
    }
}
