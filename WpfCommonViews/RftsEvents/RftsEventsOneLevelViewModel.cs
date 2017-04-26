using System.Data;
using System.Linq;
using Iit.Fibertest.StringResources;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;

namespace Iit.Fibertest.WpfCommonViews
{
    public class RftsEventsOneLevelViewModel
    {
        public DataTable BindableTable { get; set; }
        private EventsContent EventsContent { get; set; }

        public bool IsFailed { get; set; }

        public RftsEventsOneLevelEeltViewModel EeltViewModel { get; set; }

        public RftsEventsOneLevelViewModel(OtdrDataKnownBlocks sorData, RftsLevel rftsLevel)
        {
            EventsContent = new SorDataToEvents(sorData).Parse(rftsLevel.LevelName);
            CreateTable(EventsContent.Table.First().Value.Length-1);
            PopulateTable();
            EeltViewModel = new RftsEventsOneLevelEeltViewModel(sorData.KeyEvents.EndToEndLoss, rftsLevel.EELT, sorData.RftsEvents.EELD);
            IsFailed = EventsContent.IsFailed || EeltViewModel.IsFailed;
        }

        private void CreateTable(int eventCount)
        {
            BindableTable = new DataTable();
            BindableTable.Columns.Add(new DataColumn(Resources.SID_Parameters));
            for (int i = 0; i < eventCount; i++)
                BindableTable.Columns.Add(new DataColumn(string.Format(Resources.SID_Event_N_0_, i)) { DataType = typeof(string) });
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
