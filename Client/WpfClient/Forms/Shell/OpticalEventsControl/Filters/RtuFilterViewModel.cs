using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RtuFilterViewModel : Screen
    {
        private readonly ReadModel _readModel;
        private RtuGuidFilter _selectedRow;
        public List<RtuGuidFilter> Rows { get; set; }

        public RtuGuidFilter SelectedRow
        {
            get { return _selectedRow; }
            set
            {
                if (Equals(value, _selectedRow)) return;
                _selectedRow = value;
                NotifyOfPropertyChange();
            }
        }



        public RtuFilterViewModel(ReadModel readModel)
        {
            _readModel = readModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Select_RTU;
        }

        public void Initialize()
        {
            Rows = new List<RtuGuidFilter> {new RtuGuidFilter()};
            foreach (var rtu in _readModel.Rtus)
            {
                if (!string.IsNullOrEmpty(rtu.Title))
                    Rows.Add(new RtuGuidFilter(rtu.Id, rtu.Title));
            }

            SelectedRow = Rows.First();
        }

        public void Apply()
        {
            TryClose(true);
        }
    }
}
