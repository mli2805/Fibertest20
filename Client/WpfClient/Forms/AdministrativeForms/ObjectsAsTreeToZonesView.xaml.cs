using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    /// <summary>
    /// Interaction logic for ObjectsAsTreeToZonesView.xaml
    /// </summary>
    public partial class ObjectsAsTreeToZonesView
    {
        private bool _isDataGridConstructorUsed;
        private ObjectsAsTreeToZonesViewModel _vm;

        public ObjectsAsTreeToZonesView()
        {
            InitializeComponent();
            // DataContext is not set yet
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            // Yes, DataContext is set now!
            if (DataContext == null) return;
            _vm = (ObjectsAsTreeToZonesViewModel)DataContext;
            ConstructDataGrid();
        }


        private void ConstructDataGrid()
        {
            if (_isDataGridConstructorUsed) return;
            _isDataGridConstructorUsed = true;

            var columntTitle = new DataGridTextColumn
            {
                Header = StringResources.Resources.SID_Subject,
                Width = 200,
                Binding = new Binding(@"SubjectTitle")
            };
            MainDataGrid.Columns.Add(columntTitle);

            var index = 0;

            foreach (var zone in _vm.ReadModel.Zones)
            {
                var columnZone = new DataGridTemplateColumn
                {
                    HeaderTemplate = GetZoneColumnHeaderTemplate(zone),
                    Width = 150,
                    CellTemplate = GetZoneColumnCellTemplate(index, zone)
                };
                MainDataGrid.Columns.Add(columnZone);
                index++;
            }

            MainDataGrid.ItemsSource = _vm.Rows;
            MainDataGrid.SetBinding(Selector.SelectedItemProperty, new Binding(@"SelectedRow"));
        }

        private DataTemplate GetZoneColumnCellTemplate(int index, Zone zone)
        {
            var cellTempate = new DataTemplate {DataType = typeof(ObjectToZonesModel)};
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
            FrameworkElementFactory isZoneIncluded = new FrameworkElementFactory(typeof(CheckBox));
            isZoneIncluded.SetBinding(ToggleButton.IsCheckedProperty, new Binding($@"IsInZones[{index}].IsChecked"));
            var margin = new Thickness(62, 0, 0, 0);
            isZoneIncluded.SetValue(MarginProperty, margin);
            isZoneIncluded.SetValue(TagProperty, index);
            isZoneIncluded.SetValue(IsEnabledProperty, !zone.IsDefaultZone);

            isZoneIncluded.AddHandler(ButtonBase.ClickEvent, (RoutedEventHandler)CheckBoxClicked);

            borderFactory.AppendChild(isZoneIncluded);
            cellTempate.VisualTree = borderFactory;
            return cellTempate;
        }

        private DataTemplate GetZoneColumnHeaderTemplate(Zone zone)
        {
            var headerTemplate = new DataTemplate();

            FrameworkElementFactory gridAroundTextblock = new FrameworkElementFactory(typeof(Grid));
            FrameworkElementFactory zoneColumnHeader = new FrameworkElementFactory(typeof(TextBlock));
            zoneColumnHeader.SetValue(TextBlock.TextProperty, zone.Title);
            zoneColumnHeader.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            zoneColumnHeader.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
            zoneColumnHeader.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            var headerMargin = new Thickness(5, 0, 0, 0);
            zoneColumnHeader.SetValue(MarginProperty, headerMargin);

            gridAroundTextblock.AppendChild(zoneColumnHeader);
            headerTemplate.VisualTree = gridAroundTextblock;
            return headerTemplate;
        }

        private void CheckBoxClicked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var column = (int)checkBox.Tag;
            if (_vm.SelectedRow.IsRtu)
            {
                var rtu = _vm.ReadModel.Rtus.First(r => r.Id == _vm.SelectedRow.RtuId);
                // change this zone for all rtu's traces
                foreach (var lineModel in _vm.Rows.Where(l => !l.IsRtu && l.RtuId == rtu.Id))
                {
                    lineModel.IsInZones[column].IsChecked = checkBox.IsChecked == true;
                }
            }
            else
            {
                var trace = _vm.ReadModel.Traces.First(t => t.Id == _vm.SelectedRow.TraceId);
                // if Trace checked RTU should be checked too
                if (checkBox.IsChecked == true)
                {
                    var rtu = _vm.ReadModel.Rtus.First(r => r.Id == trace.RtuId);
                    var rtuLine = _vm.Rows.First(l => l.RtuId == rtu.Id);
                    rtuLine.IsInZones[column].IsChecked = true;
                }
            }
        }

        private void MainDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            DataGridRow row = e.Row;
            if (!(row.Item is ObjectToZonesModel lineModel))
                return;

            if (!lineModel.SubjectTitle.StartsWith(@"  "))
            {
                row.FontWeight = FontWeights.Bold;
                row.Background = Brushes.LightCyan;
            }
        }

       
    }
}
