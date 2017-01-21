using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Iit.Fibertest.WpfClient.ViewModels;

namespace Graph.Tests
{
    public class FakeWindowManager : IWindowManager
    {
        public List<object> Log = new List<object>();
        public bool? ShowDialog(object rootModel, object context = null, IDictionary<string, object> settings = null)
        {
            _del?.Invoke(rootModel);

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

        public delegate void DelegateHandler(object model);

        DelegateHandler _del;

        public void RegisterHandler(DelegateHandler del)
        {
            _del = del;
        }
    }
}
