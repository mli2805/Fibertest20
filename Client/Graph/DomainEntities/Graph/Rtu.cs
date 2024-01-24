using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class Rtu
    {
        public Guid Id { get; set; }

        // public string OtauId { get; set; } // in VeEX RTU main OTAU has its own ID
        public string OtdrId { get; set; } // ditto
        public VeexOtau MainVeexOtau { get; set; } = new VeexOtau(); // in Veex RTU it is a separate unit

        public RtuMaker RtuMaker { get; set; }
        public string Mfid { get; set; }
        public string Mfsn { get; set; }
        public string Omid { get; set; }
        public string Omsn { get; set; }

        public Guid NodeId { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }

        public NetAddress MainChannel { get; set; } = new NetAddress(@"", -1);
        public RtuPartState MainChannelState { get; set; }
        public NetAddress ReserveChannel { get; set; } = new NetAddress("", -1);
        public RtuPartState ReserveChannelState { get; set; }
        public bool IsReserveChannelSet { get; set; } = false;
        public NetAddress OtdrNetAddress { get; set; } = new NetAddress(@"0.0.0.0", 1500); // real address
        public bool IsAvailable => MainChannelState == RtuPartState.Ok ||
                                   ReserveChannelState == RtuPartState.Ok;
        public bool IsAllRight => MainChannelState == RtuPartState.Ok &&
                                  ReserveChannelState != RtuPartState.Broken;

        // pair OTAU ID - is OK or not
        private Dictionary<Guid, bool> _otauStates = new Dictionary<Guid, bool>();
        public void SetOtauState(Guid otauId, bool isOk)
        {
            if (_otauStates == null)
                _otauStates = new Dictionary<Guid, bool>();
            _otauStates[otauId] = isOk;
        }

        public void RemoveOtauState(Guid otauId)
        {
            if (_otauStates == null)
                _otauStates = new Dictionary<Guid, bool>();
            if (_otauStates.ContainsKey(otauId))
                _otauStates.Remove(otauId);
        }

        public RtuPartState BopState
        {
            get
            {
                if (_otauStates == null)
                    _otauStates = new Dictionary<Guid, bool>();
                return _otauStates.Count == 0
                    ? RtuPartState.NotSetYet
                    : _otauStates.Any(s => s.Value != true)
                        ? RtuPartState.Broken
                        : RtuPartState.Ok;
            }
        }
        
        public string Serial { get; set; }
        public int OwnPortCount { get; set; }
        public bool IsInitialized => OwnPortCount != 0;
        public int FullPortCount { get; set; }

        public string PortCount => OwnPortCount == FullPortCount ? $@"{FullPortCount}" : $@"{OwnPortCount} / {FullPortCount}";

        public string Version { get; set; }
        public string Version2 { get; set; }

        public Dictionary<int, OtauDto> Children { get; set; } = new Dictionary<int, OtauDto>();

        public MonitoringState MonitoringState { get; set; }
        public Frequency PreciseMeas { get; set; } = Frequency.EveryHour;
        public Frequency PreciseSave { get; set; } = Frequency.DoNot;
        public Frequency FastSave { get; set; } = Frequency.DoNot;

        public TreeOfAcceptableMeasParams AcceptableMeasParams { get; set; } = new TreeOfAcceptableMeasParams();

        public List<Guid> ZoneIds { get; set; } = new List<Guid>();

    }
}
