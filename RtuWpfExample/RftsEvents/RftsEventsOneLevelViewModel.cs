using System.Collections.Generic;
using System.Data;
using System.Linq;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;

namespace RtuWpfExample
{
    public class EventsContent
    {
        public Dictionary<int, string[]> Table { get; set; } = new Dictionary<int, string[]>();
        public bool IsFailed { get; set; }
    }
    public class RftsEventsOneLevelViewModel
    {
        public DataTable BindableTable { get; set; }
        private EventsContent EventsContent { get; set; }

        public bool IsFailed { get; set; }

        public RftsEventsOneLevelEeltViewModel EeltViewModel { get; set; }

        public RftsEventsOneLevelViewModel(OtdrDataKnownBlocks sorData, RftsLevel rftsLevel)
        {
            EventsContent = new SorDataParser(sorData).Parse(rftsLevel.LevelName);
            CreateTable(EventsContent.Table.First().Value.Length-1);
            PopulateTable();
            EeltViewModel = new RftsEventsOneLevelEeltViewModel(sorData.KeyEvents.EndToEndLoss, rftsLevel.EELT, sorData.RftsEvents.EELD);
            IsFailed = EventsContent.IsFailed || EeltViewModel.IsFailed;
        }

        private void CreateTable(int eventCount)
        {
            BindableTable = new DataTable();
            BindableTable.Columns.Add(new DataColumn("Parameters"));
            for (int i = 0; i < eventCount; i++)
                BindableTable.Columns.Add(new DataColumn($"Event N{i}") { DataType = typeof(string) });
        }

        private void PopulateTable()
        {
            foreach (var pair in EventsContent.Table)
            {
                DataRow newRow = BindableTable.NewRow();
                for (int i = 0; i < pair.Value.Length; i++)
                    newRow[i] = pair.Value[i];
                BindableTable.Rows.Add(newRow);
            }
        }
    }
}
