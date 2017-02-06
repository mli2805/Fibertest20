﻿using System;
using System.ComponentModel;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public class RtuUpdateViewModel : Screen, IDataErrorInfo
    {
        private readonly Guid _nodeId;
        private readonly GraphVm _graphVm;

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange();
            }
        }

        public string Comment { get; set; }

        private bool _isButtonSaveEnabled;

        public bool IsButtonSaveEnabled
        {
            get { return _isButtonSaveEnabled; }
            set
            {
                if (value == _isButtonSaveEnabled) return;
                _isButtonSaveEnabled = value;
                NotifyOfPropertyChange();
            }
        }
        public GpsInputViewModel GpsInputViewModel { get; set; }

        public UpdateRtu Request { get; set; }

        public RtuUpdateViewModel(Guid nodeId, GraphVm graphVm)
        {
            _nodeId = nodeId;
            _graphVm = graphVm;

            Initilize();
        }

        public NodeVm NodeVm { get; set; }

        private void Initilize()
        {
            var rtu = _graphVm.Rtus.First(r => r.Node.Id == _nodeId);

            var currentMode = GpsInputMode.DegreesAndMinutes; // somewhere in configuration file...
            NodeVm = rtu.Node;
            GpsInputViewModel = new GpsInputViewModel(currentMode, NodeVm.Position);

            Title = rtu.Title;
            Comment = rtu.Comment;
        }

        protected override void OnViewLoaded(object view)
        {
        }

        public void Save()
        {
            IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingViewModelToCommand>()).CreateMapper();
            Request = mapper.Map<UpdateRtu>(this);
            Request.NodeId = _nodeId;
            Request.Id = _graphVm.Rtus.First(r => r.Node.Id == _nodeId).Id;
            Request.Latitude = GpsInputViewModel.OneCoorViewModelLatitude.StringsToValue();
            Request.Longitude = GpsInputViewModel.OneCoorViewModelLongitude.StringsToValue();
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }

        public string this[string columnName]
        {
            get
            {
                var errorMessage = string.Empty;
                switch (columnName)
                {
                    case "Title":
                        if (string.IsNullOrEmpty(Title))
                            errorMessage = "Title is required";
                        if (_graphVm.Rtus.Any(n => n.Title == Title))
                            errorMessage = "There is a node with the same title";
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; set; }
    }


}
