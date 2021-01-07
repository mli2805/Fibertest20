﻿using System.Runtime.Serialization;

namespace Iit.Fibertest.Dto
{
    [DataContract]
    public class ServerAsksClientToExitDto
    {
        [DataMember]
        public bool ToAll { get; set; }
        [DataMember]
        public string ConnectionId { get; set; }
        [DataMember]
        public UnRegisterReason Reason { get; set; }
    }

    public enum UnRegisterReason
    {
        UserRegistersAnotherSession,
        DbOptimizationStarted,
        DbOptimizationFinished,
    }
}