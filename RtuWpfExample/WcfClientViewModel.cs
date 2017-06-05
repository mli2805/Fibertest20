using System.Windows;
using Caliburn.Micro;

namespace Iit.Fibertest.RtuWpfExample
{
    public class WcfClientViewModel : Screen
    {


        public void WcfTest()
        {
            ServiceReference1.Service1Client myService = new ServiceReference1.Service1Client();
            MessageBox.Show(myService.GetData(123), @"My Service");
            myService.Close();
        }
    }
}
