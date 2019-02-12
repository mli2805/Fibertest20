using System;
using System.Windows;
using GMap.NET;
using Iit.Fibertest.Graph;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var res = Double.TryParse(T1.Text, out double lat1);
            if (!res) return;
            res = Double.TryParse(T2.Text, out double lng1);
            if (!res) return;
            res = Double.TryParse(T3.Text, out double lat2);
            if (!res) return;
            res = Double.TryParse(T4.Text, out double lng2);
            if (!res) return;

            var p1 = new PointLatLng(lat1, lng1);
            var p2 = new PointLatLng(lat2, lng2);
            DOld.Text = $"{GisLabCalculator.GetDistanceBetweenPointLatLngOldMethod(p1, p2):#,0} m";
            var distNew = GisLabCalculator.GetDistanceBetweenPointLatLng(p1, p2, out double azimut);
            DNew.Text = $"{distNew:#,0} m ; azimuth = {azimut:0.000} рад или {azimut / Math.PI * 180 :0.00} гр";

            var point = GisLabCalculator.GetPointAsPartOfSegment(p1, p2, 0.50);
            Middle.Text = $"50% = {point.Lat:0.00} : {point.Lng:0.00}";
        }


        
    }
}
