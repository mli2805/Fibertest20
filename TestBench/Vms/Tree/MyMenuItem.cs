using System.Collections.Generic;
using System.Windows.Input;

namespace Iit.Fibertest.TestBench
{
    public class MyMenuItem
    {
        public string Header { get; set; }
        public List<MyMenuItem> Children { get; private set; }
        public ICommand Command { get; set; }
        public object CommandParameter { get; set; }

        public MyMenuItem()
        {
            Children = new List<MyMenuItem>();
        }
    }
}