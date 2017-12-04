using System.Collections.Generic;
using System.Dynamic;
using System.Windows;
using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class MyWindowManager : IMyWindowManager
    {
        private readonly IWindowManager _windowManager;

        public MyWindowManager(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public bool? ShowDialog(object rootModel)
        {
            dynamic settings = new ExpandoObject();
            settings.Owner = Application.Current.MainWindow;
            return _windowManager.ShowDialog(rootModel, null, settings);
        }

        public void ShowWindow(object rootModel)
        {
            dynamic settings = new ExpandoObject();
            settings.Owner = Application.Current.MainWindow;
            _windowManager.ShowWindow(rootModel, null, settings);
        }

        public void ShowPopup(object rootModel, object context = null, IDictionary<string, object> settings = null)
        {
            throw new System.NotImplementedException();
        }
    }
}