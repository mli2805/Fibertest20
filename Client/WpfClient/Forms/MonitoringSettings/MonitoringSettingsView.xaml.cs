using System.Windows.Input;

namespace Iit.Fibertest.Client.MonitoringSettings
{
    /// <summary>
    /// Interaction logic for MonitoringSettingsView.xaml
    /// </summary>
    public partial class MonitoringSettingsView
    {
        public MonitoringSettingsView()
        {
            InitializeComponent();
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.B && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                await ((MonitoringSettingsViewModel) DataContext).ReSendBaseRefsForAllSelectedTraces();
            }

        }
    }
}
