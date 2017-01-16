using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Graph.Tests
{
    public class FakeWindowManager : IWindowManager
    {
        public List<object> Log = new List<object>();
        public bool? ShowDialog(object rootModel, object context = null, IDictionary<string, object> settings = null)
        {
            Log.Add(rootModel);
            return null;
        }

        public void ShowWindow(object rootModel, object context = null, IDictionary<string, object> settings = null)
        {
            throw new NotImplementedException();
        }

        public void ShowPopup(object rootModel, object context = null, IDictionary<string, object> settings = null)
        {
            throw new NotImplementedException();
        }
    }
}
