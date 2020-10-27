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
        public static PdfDocument Create(DataTable levelDataTable)
        {
            Document doc = new Document();
            doc.DefaultPageSetup.Orientation = Orientation.Landscape;
            doc.DefaultPageSetup.LeftMargin = Unit.FromCentimeter(2);
            doc.DefaultPageSetup.RightMargin = Unit.FromCentimeter(1);
            doc.DefaultPageSetup.TopMargin = Unit.FromCentimeter(0.5);
            doc.DefaultPageSetup.BottomMargin = Unit.FromCentimeter(1.5);
            doc.DefaultPageSetup.FooterDistance = Unit.FromCentimeter(0.5);

            Section section = doc.AddSection();
            section.PageSetup.DifferentFirstPageHeaderFooter = false;

            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText(Resources.SID_Rfts_Events, TextFormat.NotBold);
            paragraph.Format.Font.Size = 14;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.4);
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.4);

            DrawTables(levelDataTable, doc, section);

            PdfDocumentRenderer pdfDocumentRenderer =
                new PdfDocumentRenderer(true) {Document = doc};
            pdfDocumentRenderer.RenderDocument();

            return pdfDocumentRenderer.PdfDocument;
        }

        private const int eventOnPage = 6;
        private static readonly int[] separatorLines = {
            0, 8, 12, 16
        };

        private static readonly int stateLine = 3;

        private static void DrawTables(DataTable levelDataTable, Document doc, Section section)
        {
            var line = levelDataTable.Rows[0];
            var eventCount = line.ItemArray.Length - 1;
            var pages = eventCount / eventOnPage + 1;
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
            var columnCount = levelDataTable.Rows[0].ItemArray.Length - 1 - ordinal * eventOnPage;
            columnCount = columnCount > eventOnPage ? eventOnPage : columnCount;
            DrawTableHeader(table, ordinal, columnCount);

            for (int i = 0; i < levelDataTable.Rows.Count; i++)
            {
                DataRow line = levelDataTable.Rows[i];
                var row = table.AddRow();
                row.Height = Unit.FromCentimeter(0.6);
                row.HeightRule = RowHeightRule.AtLeast;
                row.VerticalAlignment = VerticalAlignment.Center;
                row.Cells[0].AddParagraph((string)line.ItemArray[0]);
                if (separatorLines.Contains(i))
                    row.Cells[0].Shading.Color = Colors.Azure;

                for (int j = 1; j <= columnCount; j++)
                {
                    var text = line.ItemArray[j + ordinal * eventOnPage] is System.DBNull
                        ? "" 
                        : line.ItemArray[j + ordinal * eventOnPage];
                    row.Cells[j].AddParagraph((string)text);
                    if (separatorLines.Contains(i))
                        row.Cells[j].Shading.Color = Colors.Azure;
                    if (i == stateLine && text.ToString() != Resources.SID_pass && text.ToString() != "")
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
                rowHeader.Cells[i + 1].AddParagraph(string.Format(Resources.SID_Event_N_0_, i + ordinal * eventOnPage));
        }

    }
}
