﻿using System;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class LandmarkRow : PropertyChangedBase
    {
        public Guid NodeId { get; set; }
        public int Number { get; set; }
        public int NumberIncludingAdjustmentPoints { get; set; }

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

        public Guid EquipmentId { get; set; }
        public string EquipmentTitle { get; set; }
        public string EquipmentType { get; set; }
        public string Distance { get; set; }
        public string EventNumber { get; set; }

        private string _gpsCoors;
        private string _nodeTitle;
        private string _nodeComment;

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

        public LandmarkRow FromLandmark(Landmark landmark, GpsInputMode mode)
        {
            Number = landmark.Number;
            NumberIncludingAdjustmentPoints = landmark.NumberIncludingAdjustmentPoints;
            NodeId = landmark.NodeId;
            NodeTitle = landmark.NodeTitle;
            NodeComment = landmark.NodeComment;
            EquipmentId = landmark.EquipmentId;
            EquipmentTitle = landmark.EquipmentTitle;
            EquipmentType = landmark.EquipmentType.ToLocalizedString();
            Distance = $@"{landmark.Distance: 0.000}";
            EventNumber = landmark.EventNumber == -1 ? Resources.SID_no : $@"{landmark.EventNumber}";
            GpsCoors = landmark.GpsCoors.ToDetailedString(mode);

            return this;
        }
    }
}