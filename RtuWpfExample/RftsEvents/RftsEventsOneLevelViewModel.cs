using System.Collections.Generic;
using System.Data;
using System.Linq;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;

namespace RtuWpfExample
{
    public class RftsEventsOneLevelViewModel
    {
        public DataTable BindableTable { get; set; }

        public RftsEventsOneLevelEeltViewModel EeltViewModel { get; set; }

        public RftsEventsOneLevelViewModel(OtdrDataKnownBlocks sorData, RftsLevel rftsLevel)
        {
            var lines = new SorDataParser(sorData).Parse(rftsLevel.LevelName);
            CreateTable(lines.First().Value.Length-1);
            PopulateTable(lines);
            EeltViewModel = new RftsEventsOneLevelEeltViewModel(rftsLevel, sorData.KeyEvents.EndToEndLoss);
        }

        private void PopulateTable(Dictionary<int , string[]> lines)
        {
            foreach (var pair in lines)
            {
                DataRow newRow = BindableTable.NewRow();
                for (int i = 0; i < pair.Value.Length; i++)
                    newRow[i] = pair.Value[i];
                BindableTable.Rows.Add(newRow);
            }
        }

        private void CreateTable(int eventCount)
        {
            BindableTable = new DataTable();

            DataColumn parametersColumn = new DataColumn() { ColumnName = "Parameters" };
            BindableTable.Columns.Add(parametersColumn);

            for (int i = 0; i < eventCount; i++)
            {
                BindableTable.Columns.Add(new DataColumn($"Event N{i}") { DataType = typeof(string) });
            }
        }

    }
}
