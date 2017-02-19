using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Iit.Fibertest.TestBench
{
    [Obsolete("this is a wrong approach")]
    public class MenuItemEx : MenuItem
    {
        public List<MenuItemEx> Children { get; private set; } = new List<MenuItemEx>();

    }
}