using System;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace Iit.Fibertest.Client
{
    public class LandmarkRow : PropertyChangedBase
    {
        public Guid NodeId { get; set; }
        public Guid FiberId { get; set; } // to the left
        public int Number { get; set; }
        public int NumberIncludingAdjustmentPoints { get; set; }

        private string _nodeTitle;
        public string NodeTitle
        {
            get => _nodeTitle;
            set
            {
                if (value == _nodeTitle) return;
                _nodeTitle = value;
                NotifyOfPropertyChange();
            }
        }

        public Brush NodeTitleBrush { get; set; } = Brushes.Transparent;

        private string _nodeComment;
        public string NodeComment
        {
            get => _nodeComment;
            set
            {
                if (value == _nodeComment) return;
                _nodeComment = value;
                NotifyOfPropertyChange();
            }
        }

        public Brush NodeCommentBrush { get; set; } = Brushes.Transparent;

        public Guid EquipmentId { get; set; }
        public string EquipmentTitle { get; set; }
        public Brush EquipmentTitleBrush { get; set; } = Brushes.Transparent;
        public string EquipmentType { get; set; }
        public Brush EquipmentTypeBrush { get; set; } = Brushes.Transparent;
        public string CableReserves { get; set; }
        public Brush CableReservesBrush { get; set; } = Brushes.Transparent;
        public string GpsDistance { get; set; } // by GPS, ignore cable reserve
        public string GpsSection { get; set; }
        public bool IsUserInput { get; set; }
        public Brush GpsSectionBrush { get; set; }
        public string OpticalDistance { get; set; } // from sor
        public string OpticalSection { get; set; }
        public string EventNumber { get; set; }

        private string _gpsCoors;
        public string GpsCoors
        {
            get => _gpsCoors;
            set
            {
                if (value == _gpsCoors) return;
                _gpsCoors = value;
                NotifyOfPropertyChange();
            }
        }
        public Brush GpsCoorsBrush { get; set; } = Brushes.Transparent;

        public LandmarkRow FromLandmark(Landmark landmark, Landmark oldLandmark, GpsInputMode mode)
        {
            Number = landmark.Number;
            NumberIncludingAdjustmentPoints = landmark.NumberIncludingAdjustmentPoints;
            NodeId = landmark.NodeId;
            FiberId = landmark.FiberId;
            NodeTitle = landmark.NodeTitle ?? "";
            NodeTitleBrush = landmark.NodeTitle == oldLandmark.NodeTitle ? Brushes.Transparent : Brushes.Cornsilk;
            NodeComment = landmark.NodeComment;
            EquipmentId = landmark.EquipmentId;
            EquipmentTitle = landmark.EquipmentTitle ?? "";
            EquipmentTitleBrush = landmark.EquipmentTitle == oldLandmark.EquipmentTitle
                ? Brushes.Transparent
                : Brushes.Cornsilk;
            EquipmentType = landmark.EquipmentType.ToLocalizedString();
            EquipmentTypeBrush = landmark.EquipmentType == oldLandmark.EquipmentType
                ? Brushes.Transparent
                : Brushes.Cornsilk;
            CableReserves = CableReserveToString(landmark);
            CableReservesBrush = CableReserveToString(landmark) == CableReserveToString(oldLandmark)
                ? Brushes.Transparent
                : Brushes.Cornsilk;
            GpsDistance = $@"{landmark.GpsDistance: 0.000}";
            GpsSection = landmark.EquipmentType == Dto.EquipmentType.Rtu ? "" : $@"{landmark.GpsSection: 0.000}";
            GpsSectionBrush = landmark.IsUserInput 
                ? Math.Abs(landmark.GpsSection - oldLandmark.GpsSection) < 0.0005 
                    ? Brushes.LightGray : Brushes.Cornsilk 
                : Brushes.Transparent;
            IsUserInput = landmark.IsUserInput;
            OpticalDistance = landmark.IsFromBase ? $@"{landmark.OpticalDistance: 0.000}" : "";
            OpticalSection = landmark.EquipmentType == Dto.EquipmentType.Rtu ? "" 
                : landmark.IsFromBase ? $@"{landmark.OpticalSection: 0.000}" : "";
            EventNumber = landmark.EventNumber == -1 ? Resources.SID_no : $@"{landmark.EventNumber}";
            GpsCoors = landmark.GpsCoors.ToDetailedString(mode);
            GpsCoorsBrush = landmark.GpsCoors == oldLandmark.GpsCoors ? Brushes.Transparent : Brushes.Cornsilk;

            return this;
        }

        private string CableReserveToString(Landmark landmark)
        {
            if (landmark.EquipmentType == Dto.EquipmentType.CableReserve)
            {
                return $@"{landmark.LeftCableReserve}";
            } 

            if (landmark.EquipmentType > Dto.EquipmentType.CableReserve &&
                     landmark.EquipmentType < Dto.EquipmentType.RtuAndEot)
            {
                return $@"{landmark.LeftCableReserve} / {landmark.RightCableReserve}";
            }

            return "";
        }
    }
}