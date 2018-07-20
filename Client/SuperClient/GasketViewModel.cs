using System.Collections.ObjectModel;
using System.Windows.Controls;
using Caliburn.Micro;

namespace Iit.Fibertest.SuperClient
{
    public class GasketViewModel : PropertyChangedBase
    {   
        public ObservableCollection<TabItem> Children { get; set; } = new ObservableCollection<TabItem>();
        private int _selectedTabItemIndex;
        public int SelectedTabItemIndex
        {
            get => _selectedTabItemIndex;
            set
            {
                if (_selectedTabItemIndex == value) return;
                _selectedTabItemIndex = value;
                NotifyOfPropertyChange();
            }
        }
      
    }
}
