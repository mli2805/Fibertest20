﻿using System;
using System.Windows.Media;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class BopNetworkEventModel
    {
        public int Nomer { get; set; }
        public DateTime EventTimestamp { get; set; }

        public string Serial { get; set; }
        public string OtauIp { get; set; }
        public int TcpPort { get; set; }

        public string RtuTitle { get; set; }
        public Guid RtuId { get; set; }

        public bool IsOk { get; set; }

        public string StateString => IsOk ? RtuPartState.Ok.ToLocalizedString() : RtuPartState.Broken.ToLocalizedString();
        public Brush StateBrush => IsOk ? RtuPartState.Ok.GetBrush(false) : RtuPartState.Broken.GetBrush(false);

    }
}
