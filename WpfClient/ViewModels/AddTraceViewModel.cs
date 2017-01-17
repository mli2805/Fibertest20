using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public class AddTraceViewModel : Screen, IDataErrorInfo
    {
        private readonly IWindowManager _windowManager;
        private readonly ReadModel _readModel;
        private readonly Aggregate _aggregate;
        private readonly List<Guid> _nodes;
        private readonly List<Guid> _equipments;
        private string _title;
        private string _comment;

        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(nameof(IsButtonSaveEnabled));
            }
        }

        public string Comment
        {
            get { return _comment; }
            set
            {
                if (value == _comment) return;
                _comment = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsButtonSaveEnabled => !string.IsNullOrEmpty(_title);

        public bool IsClosed { get; set; }


        public AddTraceViewModel(IWindowManager windowManager, ReadModel readModel, Aggregate aggregate, List<Guid> nodes, List<Guid> equipments)
        {
            _windowManager = windowManager;
            _readModel = readModel;
            _aggregate = aggregate;
            _nodes = nodes;
            _equipments = equipments;

            IsClosed = false;
        }

        public void Save()
        {
            var error = _aggregate.When(new AddTrace()
            {
                Id = Guid.NewGuid(),
                RtuId = _readModel.Rtus.Single(r=>r.NodeId == _nodes[0]).Id,
                Title = _title,
                Nodes = _nodes,
                Equipments = _equipments,
                Comment = _comment
            });

            if (error != null)
            {
                _windowManager.ShowDialog(new ErrorNotificationViewModel(error));
                return;
            }

            CloseView();
        }

        public void Cancel()
        {
            CloseView();
        }

        private void CloseView()
        {
            IsClosed = true;
            TryClose();
        }

        public string this[string columnName]
        {
            get
            {
                String errorMessage = String.Empty;
                switch (columnName)
                {
                    case "Title":
                        if (String.IsNullOrEmpty(Title))
                        {
                            errorMessage = "Title is required";
                        }
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; }
    }
}
