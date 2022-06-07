using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Caliburn.Micro;
using PdfSharp.Pdf;

namespace Iit.Fibertest.WpfCommonViews
{
    public static class PdfExposer
    {
        public static void Show(PdfDocument report, string filename, IWindowManager windowManager)
        {
            try
            {
                var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\Reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string path = Path.Combine(folder, filename);
                report.Save(path);
                Process.Start(path);
            }
            catch (Exception e)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>(){ @"Show PDF report error:", e.Message}, 1);
                windowManager.ShowDialogWithAssignedOwner(vm);
            }
        }
    }
}