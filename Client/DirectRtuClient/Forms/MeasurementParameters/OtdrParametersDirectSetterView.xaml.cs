﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DirectRtuClient
{
    /// <summary>
    /// Interaction logic for OtdrParametersDirectSetterView.xaml
    /// </summary>
    public partial class OtdrParametersDirectSetterView
    {
        public OtdrParametersDirectSetterView()
        {
            InitializeComponent();
        }

        private void RadioButtonOnChecked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            RbCount.Foreground  = rb.Name == @"RbCount" ? Brushes.Black : Brushes.DarkGray;
            CbCounts.Foreground = rb.Name == @"RbCount" ? Brushes.Black : Brushes.DarkGray;
            RbTime.Foreground   = rb.Name == @"RbCount" ? Brushes.DarkGray : Brushes.Black;
            CbTimes.Foreground  = rb.Name == @"RbCount" ? Brushes.DarkGray : Brushes.Black;
        }
    }
}
