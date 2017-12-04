using System.Collections.Generic;

namespace Iit.Fibertest.Client
{
    public interface IMyWindowManager
    {
        bool? ShowDialog(object rootModel);
        void ShowWindow(object rootModel);
        void ShowPopup(object rootModel, object context = null, IDictionary<string, object> settings = null);
    }
}