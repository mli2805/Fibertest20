using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;

namespace Iit.Fibertest.WpfCommonViews
{
    public static class WindowManagerExt
    {
        public static bool? ShowDialogWithAssignedOwner(this IWindowManager windowManager, object model)
        {
            var setting = new Dictionary<string, object>()
            {
                [@"Owner"] = Application.Current?.MainWindow
            };
            return windowManager.ShowDialog(model, null, setting);
        }

        public static void ShowWindowWithAssignedOwner(this IWindowManager windowManager, object model)
        {
            var setting = new Dictionary<string, object>()
            {
                [@"Owner"] = Application.Current?.MainWindow
            };
            windowManager.ShowWindow(model, null, setting);
        }
    }
}