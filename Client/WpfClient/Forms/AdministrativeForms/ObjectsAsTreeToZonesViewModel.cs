using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class ObjectsAsTreeToZonesViewModel : Screen
    {
        private bool _isDataGridConstructorUsed;
        public ReadModel ReadModel { get; }
        public List<ObjectToZonesModel> Rows { get; set; } = new List<ObjectToZonesModel>();
        public ObjectToZonesModel SelectedRow { get; set; }

        public ObjectsAsTreeToZonesViewModel(ReadModel readModel)
        {
            ReadModel = readModel;

            FillInRows();
        }

        public void ConstructDataGrid(DataGrid mainDataGrid)
        {
            if (_isDataGridConstructorUsed) return;
            _isDataGridConstructorUsed = true;

            var columntTitle = new DataGridTextColumn()
            {
                Header = "Subject",
                Width = 200,
                Binding = new Binding("ObjectTitle"),
            };
            mainDataGrid.Columns.Add(columntTitle);

            var index = 0;

            foreach (var zone in ReadModel.Zones)
            {
                var cellTempate = new DataTemplate() { DataType = typeof(ObjectToZonesModel) };

                FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));

                FrameworkElementFactory isZoneIncluded = new FrameworkElementFactory(typeof(CheckBox));
                isZoneIncluded.SetBinding(ToggleButton.IsCheckedProperty, new Binding($@"IsInZones[{index}].IsChecked"));
                isZoneIncluded.SetValue(FrameworkElement.TagProperty, index);

                isZoneIncluded.AddHandler(ButtonBase.ClickEvent, (RoutedEventHandler) CheckBoxClicked);

                borderFactory.AppendChild(isZoneIncluded);
                cellTempate.VisualTree = borderFactory;

                var columnZone = new DataGridTemplateColumn()
                {
                    Header = zone.Title,
                    Width = 150,
                    CellTemplate = cellTempate,
                };
                mainDataGrid.Columns.Add(columnZone);
                index++;
            }

            mainDataGrid.ItemsSource = Rows;
            mainDataGrid.SetBinding(Selector.SelectedItemProperty, new Binding(@"SelectedRow"));
        }

        private void CheckBoxClicked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox) sender;
            var column = (int)checkBox.Tag;
            if (SelectedRow.IsRtu)
            {
                var rtu = ReadModel.Rtus.First(r => r.Id == SelectedRow.ObjectId);
                // change this zone for all rtu's traces
                foreach (var lineModel in Rows.Where(l=>!l.IsRtu))
                {
                }
            }
            else
            {
                var trace = ReadModel.Traces.First(t => t.Id == SelectedRow.ObjectId);
                // if Trace checked RTU should be checked too
                if (checkBox.IsChecked == true)
                {
                    var rtu = ReadModel.Rtus.First(r => r.Id == trace.RtuId);
                    var rtuLine = Rows.First(l => l.ObjectId == rtu.Id);
                    rtuLine.IsInZones[column].IsChecked = true;
                }
            }
        }

        private void FillInRows()
        {
            foreach (var rtu in ReadModel.Rtus)
            {
                Rows.Add(RtuToLine(rtu));
                foreach (var trace in ReadModel.Traces.Where(t => t.RtuId == rtu.Id))
                    Rows.Add(TraceToLine(trace));
            }
        }

        private ObjectToZonesModel RtuToLine(Rtu rtu)
        {
            var rtuLine = new ObjectToZonesModel()
            {
                ObjectTitle = rtu.Title,
                ObjectId = rtu.Id,
                IsRtu = true,
            };
            foreach (var zone in ReadModel.Zones)
                rtuLine.IsInZones.Add(new BoolWithNotification(){IsChecked = rtu.ZoneIds.Contains(zone.ZoneId)});
            return rtuLine;
        }

        private ObjectToZonesModel TraceToLine(Trace trace)
        {
            var traceLine = new ObjectToZonesModel()
            {
                ObjectTitle = @"  " + trace.Title,
                ObjectId = trace.Id,
                IsRtu = false,
            };
            foreach (var zone in ReadModel.Zones)
                traceLine.IsInZones.Add(new BoolWithNotification() { IsChecked = trace.ZoneIds.Contains(zone.ZoneId)});

            return traceLine;
        }

        public void Save()
        {
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
