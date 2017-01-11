using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public class UpdateNodeViewModel : Screen, IDataErrorInfo
    {
        private readonly ReadModel _readModel;
        private readonly Aggregate _aggregate;

        private readonly Node _originalNode;
        public bool IsClosed { get; set; }

        public Guid Id { get; set; }

        private string _title;
        private string _comment;
        private bool _isButtonSaveEnabled;

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

        private bool IsChanged()
        {
            if (_title != _originalNode.Title)
                return true;
            return
                false;
        }

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

        public UpdateNodeViewModel(Guid id, ReadModel readModel, Aggregate aggregate)
        {
            _readModel = readModel;
            _aggregate = aggregate;
            Id = id;
            _originalNode = readModel.Nodes.Single(n => n.Id == id);
            IsClosed = false;
        }

        public void RemoveEquipment(Guid id)
        {
            var traces = _readModel.Traces.Where(t => t.Equipments.Contains(id)).ToList();
            if (traces.Any(t => t.HasBase))
                return;
            foreach (var trace in traces)
            {
                var idx = trace.Equipments.IndexOf(id);
                trace.Equipments[idx] = Guid.Empty;
            }
            _readModel.Equipments.Remove(_readModel.Equipments.Single(e => e.Id == id));
        }


        public void Save()
        {
            if (!IsChanged())
            {
                CloseView();
                return;
            }

            Error = _aggregate.When(new UpdateNode
            {
                Id = Id,
                Title = _title
            });
            if (Error != null)
                return;

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
                var  errorMessage = string.Empty;
                switch (columnName)
                {
                    case "Title":
                        if (string.IsNullOrEmpty(_title))
                            errorMessage = "Title is required";
                        if (_readModel.Nodes.Any(n=>n.Title == _title))
                            errorMessage = "There is a node with the same title";
                        IsButtonSaveEnabled = errorMessage == string.Empty;
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; set; }

    }
}
