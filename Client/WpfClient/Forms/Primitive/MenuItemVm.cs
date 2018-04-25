using System.Collections.Generic;
using System.Windows.Input;

namespace Iit.Fibertest.Client
{
    public class MenuItemVm
    {
        public string Header { get; set; }
        public List<MenuItemVm> Children { get; private set; }
        public ICommand Command { get; set; }
        public object CommandParameter { get; set; }
        public bool IsChecked { get; set; }


        public MenuItemVm()
        {
            Children = new List<MenuItemVm>();
        }
    }
}