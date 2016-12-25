using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public class UpdateNodeViewModel : Screen, IDataErrorInfo
    {
        private readonly ReadModel _model;
        private readonly Aggregate _aggregate;
        private string _title;

        private readonly Node _originalNode;
        public Guid Id { get; set; }
        public bool IsClosed { get; set; }

        public UpdateNodeViewModel(Guid id, ReadModel model, Aggregate aggregate)
        {
            _model = model;
            _aggregate = aggregate;
            Id = id;
            _originalNode = model.Nodes.Single(n => n.Id == id);
            IsClosed = false;
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

        public void Save()
        {
            if (!IsChanged())
            {
                CloseView();
                return;
            }

            Error = _aggregate.When(new UpdateNode
            {
                Title = _title
            });
            if (Error != null)
                return;

            CloseView();
        }

        private void CloseView()
        {
            IsClosed = true;
            TryClose();
        }

        public string this[string columnName]
        {
            get { throw new NotImplementedException(); }
        }

        public string Error { get; set; }

        private bool IsChanged()
        {
            if (_title != _originalNode.Title)
                return true;
            return
                false;
        }
    }
}
