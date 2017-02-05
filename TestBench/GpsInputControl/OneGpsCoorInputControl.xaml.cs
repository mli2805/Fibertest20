using System.Windows;

namespace Iit.Fibertest.TestBench
{
    /// <summary>
    /// Interaction logic for OneGpsCoorInputControl.xaml
    /// </summary>
    public partial class OneGpsCoorInputControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double),
                typeof(OneGpsCoorInputControl), new FrameworkPropertyMetadata(0.0));

        public double Value
        {
            get { return (double) GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value);
                ApplyValue(value);
            }
        }


        public static readonly DependencyProperty InputModeProperty =
            DependencyProperty.Register("InputMode", typeof(GpsInputMode),
                typeof(OneGpsCoorInputControl), new FrameworkPropertyMetadata(GpsInputMode.Degrees));

        public GpsInputMode InputMode
        {
            get { return (GpsInputMode) GetValue(InputModeProperty); }
            set { SetValue(InputModeProperty, value); }
        }


        public static readonly DependencyProperty LatOrLngProperty =
            DependencyProperty.Register("LatOrLng", typeof(LatOrLng),
                typeof(OneGpsCoorInputControl), new FrameworkPropertyMetadata(LatOrLng.Latitude));

        public LatOrLng LatOrLng
        {
            get { return (LatOrLng) GetValue(LatOrLngProperty); }
            set
            {
                SetValue(LatOrLngProperty, value);
                ApplyCoor(value);
            }
        }

        public string Degrees { get; set; }
        public string Minutes { get; set; }
        public string Seconds { get; set; }
        public string LeftRadioButtonLabel { get; set; }
        public string RightRadioButtonLabel { get; set; }

        public OneGpsCoorInputControl()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += OneGpsCoorInputControlLoaded;
        }

        private void OneGpsCoorInputControlLoaded(object sender, RoutedEventArgs e)
        {
            //Degrees.Focus();
            ApplyCoor(LatOrLng);
            ApplyInputMode(InputMode);
        }

        private void ApplyCoor(LatOrLng latOrLng)
        {
            if (latOrLng == LatOrLng.Latitude)
            {
                LeftRadioButtonLabel  = "N";
                RightRadioButtonLabel = "S";
            }
            else
            {
                LeftRadioButtonLabel = "E";
                RightRadioButtonLabel = "W";
            }
        }

        private void ApplyInputMode(GpsInputMode mode)
        {
            switch (mode)
            {
                case GpsInputMode.Degrees:
                    DegreesMode.Visibility = Visibility.Visible;
                    DegreesAndMinutesMode.Visibility = Visibility.Collapsed;
                    DegreesMinutesAndSecondsMode.Visibility = Visibility.Collapsed;
                    break;
                case GpsInputMode.DegreesAndMinutes:
                    DegreesMode.Visibility = Visibility.Collapsed;
                    DegreesAndMinutesMode.Visibility = Visibility.Visible;
                    DegreesMinutesAndSecondsMode.Visibility = Visibility.Collapsed;
                    break;
                case GpsInputMode.DegreesMinutesAndSeconds:
                    DegreesMode.Visibility = Visibility.Collapsed;
                    DegreesAndMinutesMode.Visibility = Visibility.Collapsed;
                    DegreesMinutesAndSecondsMode.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void ApplyValue(double value)
        {

        }
    }
}