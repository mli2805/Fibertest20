using System.Collections.Generic;
using System.Windows.Controls;

namespace Iit.Fibertest.TestBench
{
    public class MenuItemEx : MenuItem
    {
        public List<MenuItemEx> Children { get; private set; } = new List<MenuItemEx>();

    }
}