using System.ComponentModel;
using System.Windows;

namespace Iit.Fibertest.Client
{
    public partial class ShellView
    {
        private double _lastNonZeroWidth = 420;

        public ShellView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Применить начальное состояние после загрузки View
            ApplyRtuPanelWidth();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is INotifyPropertyChanged oldVm)
                oldVm.PropertyChanged -= ViewModel_PropertyChanged;

            if (e.NewValue is INotifyPropertyChanged newVm)
                newVm.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ShellViewModel.RtuPanelWidth))
            {
                ApplyRtuPanelWidth();
            }
        }

        private void ApplyRtuPanelWidth()
        {
            if (DataContext is ShellViewModel vm)
            {
                if (vm.RtuPanelWidth == 0)
                {
                    if (RtuPanelColumn.Width.Value > 0)
                        _lastNonZeroWidth = RtuPanelColumn.Width.Value;
                    RtuPanelColumn.Width = new GridLength(0);
                }
                else
                {
                    RtuPanelColumn.Width = new GridLength(_lastNonZeroWidth);
                }
            }
        }
    }
}