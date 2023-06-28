using System;
using System.Linq;
using System.Windows.Media;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class RtuAccidentModel
    {
        public int Id { get; set; }
        private ReturnCode _returnCode;
        public DateTime EventRegistrationTimestamp { get; set; }

        public Guid RtuId;
        public string RtuTitle { get; set; }

        public Guid TraceId;
        public string TraceTitle { get; set; }

        public BaseRefType BaseRefType;


        public string State { get; set; }
        public Brush StateBrush { get; set; }

        public RtuAccidentModel Build(RtuAccident accident, Model readModel)
        {
            Id = accident.Id;
            _returnCode = accident.ReturnCode;
            EventRegistrationTimestamp = accident.EventRegistrationTimestamp;
            RtuId = accident.RtuId;
            RtuTitle = readModel.Rtus.FirstOrDefault(r=>r.Id == accident.RtuId)?.Title ?? @"not found";
            TraceId = accident.TraceId;
            TraceTitle = readModel.Traces.FirstOrDefault(t=>t.TraceId == accident.TraceId)?.Title ?? @"not found";
            BaseRefType = accident.BaseRefType;

            SetStateAndBrush();
            return this;
        }

        private void SetStateAndBrush()
        {
            switch (_returnCode)
            {
                case ReturnCode.MeasurementEndedNormally:
                    State = _returnCode.GetLocalizedString();
                    StateBrush = Brushes.Transparent;
                    break;
                case ReturnCode.MeasurementBaseRefNotFound:
                    State = $@"{BaseRefType.GetLocalizedFemaleString()} {_returnCode.GetLocalizedString()}";
                    StateBrush = FiberState.Major.GetBrush(false);
                    break;
                case ReturnCode.MeasurementFailedToSetParametersFromBase:
                    State = string.Format(_returnCode.GetLocalizedString(), BaseRefType.GetLocalizedGenitiveString());
                    StateBrush = FiberState.Major.GetBrush(false);
                    break;
                default: 
                    State = @"Unknown type of accident"; 
                    StateBrush = Brushes.Red;
                    break;
            }
            
        }
    }
}
