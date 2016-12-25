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

        private Node _originalNode;
        public Guid Id { get; set; }

        public UpdateNodeViewModel(Guid id, ReadModel model, Aggregate aggregate)
        {
            _model = model;
            _aggregate = aggregate;
            Id = id;
            _originalNode = model.Nodes.Single(n => n.Id == id);
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
            if (IsChanged())
            _aggregate.When(new UpdateNode
            {
                
            });
        }

        public string this[string columnName]
        {
            get { throw new NotImplementedException(); }
        }

        public string Error { get; }

        private bool IsChanged()
        {
            if (_title != _originalNode.Title) return true;
            return false;
        }
    }
}
