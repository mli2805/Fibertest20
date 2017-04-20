using System;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class EqItemVm : PropertyChangedBase
    {
        private string _title;
        private string _type;
        private string _comment;
        private string _traces;
        private bool _isRemoveEnabled;
        private object _command;

        public Guid Id { get; set; }

        public string Type
        {
            get { return _type; }
            set
            {
                if (value == _type) return;
                _type = value;
                NotifyOfPropertyChange();
            }
        }

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

        public string Traces
        {
            get { return _traces; }
            set
            {
                if (value == _traces) return;
                _traces = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsRemoveEnabled
        {
            get { return _isRemoveEnabled; }
            set
            {
                if (value == _isRemoveEnabled) return;
                _isRemoveEnabled = value;
                NotifyOfPropertyChange();
            }
        }

        public object Command
        {
            get { return _command; }
            set
            {
                if (Equals(value, _command)) return;
                _command = value;
                NotifyOfPropertyChange();
            }
        }

        // context menu
        public void UpdateEquipment()
        {
            Command = new UpdateEquipment { Id = Id };
        }

        public void RemoveEquipment()
        {
            Command = new RemoveEquipment { Id = Id };
        }
    }
}