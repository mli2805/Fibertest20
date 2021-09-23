using System.Collections.Generic;
using PdfSharp.Pdf;
using System.Data;
using System.Linq;
using Iit.Fibertest.StringResources;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace Iit.Fibertest.WpfCommonViews
{
    public static class RftsEventsPdfProvider
    {
        public static PdfDocument Create(IEnumerable<RftsEventsOneLevelViewModel> levelViewModels, string traceTitle)
        {
            Document doc = new Document();
            doc.DefaultPageSetup.Orientation = Orientation.Landscape;
            doc.DefaultPageSetup.LeftMargin = Unit.FromCentimeter(2);
            doc.DefaultPageSetup.RightMargin = Unit.FromCentimeter(1);
            doc.DefaultPageSetup.TopMargin = Unit.FromCentimeter(0.5);
            doc.DefaultPageSetup.BottomMargin = Unit.FromCentimeter(1.5);
            doc.DefaultPageSetup.FooterDistance = Unit.FromCentimeter(0.5);

            foreach (var levelViewModel in levelViewModels)
            {
                Section section = doc.AddSection();
                section.PageSetup.DifferentFirstPageHeaderFooter = false;

                var paragraph = section.AddParagraph();
                paragraph.AddFormattedText(Resources.SID_Rfts_Events + $":  {traceTitle}", TextFormat.NotBold);
                paragraph.Format.Font.Size = 14;
                paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.4);
                paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.4);

                var paragraph2 = section.AddParagraph();
                paragraph2.AddFormattedText(levelViewModel.BindableTable.TableName + $":  {levelViewModel.GetState()}");
                paragraph2.Format.Font.Size = 12;
                paragraph2.Format.SpaceAfter = Unit.FromCentimeter(0.4);
                paragraph2.Format.Font.Color = levelViewModel.IsFailed ? Colors.Red : Colors.Black;

                DrawTables(levelViewModel.BindableTable, doc, section);
                DrawFooter(section, levelViewModel);
              
            }

            PdfDocumentRenderer pdfDocumentRenderer =
                new PdfDocumentRenderer(true) {Document = doc};
            pdfDocumentRenderer.RenderDocument();

            return pdfDocumentRenderer.PdfDocument;
        }

        private const int EventOnPage = 6;
        private static readonly int[] SeparatorLines = {
            0, 8, 12, 16
        };

        private static readonly int StateLine = 3;

        private static void DrawTables(DataTable levelDataTable, Document doc, Section section)
        {
            var line = levelDataTable.Rows[0];
            var eventCount = line.ItemArray.Length - 1;
            var pages = eventCount / EventOnPage + 1;
            for (int i = 0; i < pages; i++)
            {
                DrawOneTable(levelDataTable, section, i);
                if (i + 1 != pages)
                    section = doc.AddSection();
            }
        }

        private static void DrawOneTable(DataTable levelDataTable, Section section, int ordinal)
        {
            var table = section.AddTable();
            table.Borders.Width = 0.25;
            var columnCount = levelDataTable.Rows[0].ItemArray.Length - 1 - ordinal * EventOnPage;
            columnCount = columnCount > EventOnPage ? EventOnPage : columnCount;
            DrawTableHeader(table, ordinal, columnCount);

            for (int i = 0; i < levelDataTable.Rows.Count; i++)
            {
                DataRow line = levelDataTable.Rows[i];
                var row = table.AddRow();
                row.Height = Unit.FromCentimeter(0.6);
                row.HeightRule = RowHeightRule.AtLeast;
                row.VerticalAlignment = VerticalAlignment.Center;
                row.Cells[0].AddParagraph((string)line.ItemArray[0]);
                if (SeparatorLines.Contains(i))
                    row.Cells[0].Shading.Color = Colors.Azure;

                for (int j = 1; j <= columnCount; j++)
                {
                    var text = line.ItemArray[j + ordinal * EventOnPage] is System.DBNull
                        ? "" 
                        : line.ItemArray[j + ordinal * EventOnPage];
                    row.Cells[j].AddParagraph((string)text);
                    if (SeparatorLines.Contains(i))
                        row.Cells[j].Shading.Color = Colors.Azure;
                    if (i == StateLine && text.ToString() != Resources.SID_pass && text.ToString() != "")
                        row.Cells[j].Shading.Color = Colors.Red;
                }
            }

        }

        private static void DrawTableHeader(Table table, int ordinal, int columnCount)
        {
            table.AddColumn(@"7cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"3cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"3cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"3cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"3cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"3cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"3cm").Format.Alignment = ParagraphAlignment.Center;

            var rowHeader = table.AddRow();
            rowHeader.HeadingFormat = true;
            rowHeader.VerticalAlignment = VerticalAlignment.Center;
            rowHeader.TopPadding = Unit.FromCentimeter(0.1);
            rowHeader.BottomPadding = Unit.FromCentimeter(0.1);
            rowHeader.Format.Font.Bold = true;
            rowHeader.Cells[0].AddParagraph(Resources.SID_Parameters);
            for (int i = 0; i < columnCount; i++)
                rowHeader.Cells[i + 1].AddParagraph(string.Format(Resources.SID_Event_N_0_, i + ordinal * EventOnPage));
        }

        private static void DrawFooter(Section section, RftsEventsOneLevelViewModel levelViewModel)
        {
            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText(Resources.SID_Total_fiber_loss);
            paragraph.Format.Font.Size = 11;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.4);
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.1);

            var paragraph3 = section.AddParagraph();
            paragraph3.AddFormattedText($"{Resources.SID_Value__dB}  {levelViewModel.EeltViewModel.AttenuationValue}");
            paragraph3.Elements.AddSpace(5);
            paragraph3.AddFormattedText($"{Resources.SID_Threshold__dB}  {levelViewModel.EeltViewModel.Threshold}");
            paragraph3.Elements.AddSpace(5);
            paragraph3.AddFormattedText($"{Resources.SID_Deviation__dB}  {levelViewModel.EeltViewModel.DeviationValue}");
            paragraph3.Elements.AddSpace(5);
            paragraph3.AddFormattedText($"{Resources.SID_State_}  {levelViewModel.EeltViewModel.StateValue}");
            paragraph3.Format.Font.Size = 11;
            paragraph3.Format.LeftIndent = Unit.FromCentimeter(1);
            paragraph3.Format.Font.Color = levelViewModel.EeltViewModel.IsFailed ? Colors.Red : Colors.Black;
        }
    }
}
