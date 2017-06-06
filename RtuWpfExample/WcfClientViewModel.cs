using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.RtuWpfExample.ServiceReference1;

namespace Iit.Fibertest.RtuWpfExample
{
    public class WcfClientViewModel : Screen
    {


        public void WcfTest()
        {
            RtuWcfServiceClient myService = new RtuWcfServiceClient();
            MessageBox.Show(myService.GetData(123), @"My WCF Service");
            myService.Close();
        }
    }
}
