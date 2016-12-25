using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    class UpdateNodeViewModel : Screen
    {
        private string _title;
        public Guid Id { get; set; }

        public UpdateNodeViewModel(Guid id)
        {
            Id = id;
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
            
        }
    }
}
