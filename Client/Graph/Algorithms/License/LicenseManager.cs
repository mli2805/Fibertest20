using System;
using System.IO;
using Microsoft.Win32;

namespace Iit.Fibertest.Graph
{
    public class LicenseManager
    {
        public LicenseInFile ReadLicenseFromFileDialog(string initialDirectory = "")
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = @".lic";
            dlg.InitialDirectory = initialDirectory;
            dlg.Filter = @"License file  |*.lic";
            if (dlg.ShowDialog() != true) return null;

            return ReadLicenseFromFile(dlg.FileName);
        }

        public LicenseInFile ReadLicenseFromFile(string filename)
        {
            var encoded = File.ReadAllBytes(filename);
            try
            {
                return (LicenseInFile)Cryptography.Decode(encoded);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
