using System;
using System.Collections.Generic;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace Iit.Fibertest.Client
{
    public static class EventLogReportProvider
    {
        public static PdfDocument Create(List<LogLine> list)
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
            SetFooter(section);

            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText(Resources.SID_User_operations_log, TextFormat.NotBold);
            paragraph.Format.Font.Size = 14;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.4);
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.4);

            DrawTable(list, section);

            PdfDocumentRenderer pdfDocumentRenderer =
                new PdfDocumentRenderer(true) { Document = doc };
            pdfDocumentRenderer.RenderDocument();

            return pdfDocumentRenderer.PdfDocument;
        }

        private static void DrawTable(List<LogLine> list, Section section)
        {
            var table = section.AddTable();
            table.Borders.Width = 0.25;
            DrawTableHeader(table);

            foreach (var line in list)
            {
                var row = table.AddRow();
                row.Height = Unit.FromCentimeter(0.6);
                row.HeightRule = RowHeightRule.AtLeast;
                row.VerticalAlignment = VerticalAlignment.Center;
                row.Cells[0].AddParagraph(line.Ordinal.ToString());
                row.Cells[1].AddParagraph(line.Username);
                row.Cells[2].AddParagraph(line.ClientIp);
                row.Cells[3].AddParagraph($@"{line.Timestamp}");
                row.Cells[4].AddParagraph(line.OperationName);
                row.Cells[5].AddParagraph(line.RtuTitle ?? "");
                row.Cells[6].AddParagraph(line.TraceTitle ?? "");
                row.Cells[7].AddParagraph(line.OperationParams ?? "");
            }
        }

        private static void DrawTableHeader(Table table)
        {
            table.AddColumn(@"1cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"3cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"3cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"2.5cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"4.5cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"3.0cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"4cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"6cm").Format.Alignment = ParagraphAlignment.Center;

            var rowHeader = table.AddRow();
            rowHeader.HeadingFormat = true;
            rowHeader.VerticalAlignment = VerticalAlignment.Center;
            rowHeader.TopPadding = Unit.FromCentimeter(0.1);
            rowHeader.BottomPadding = Unit.FromCentimeter(0.1);
            rowHeader.Format.Font.Bold = true;
            rowHeader.Cells[0].AddParagraph(@"No");
            rowHeader.Cells[1].AddParagraph(Resources.SID_User);
            rowHeader.Cells[2].AddParagraph(Resources.SID_Client_Ip);
            rowHeader.Cells[3].AddParagraph(Resources.SID_Date);
            rowHeader.Cells[4].AddParagraph(Resources.SID_Operation);
            rowHeader.Cells[5].AddParagraph(@"RTU");
            rowHeader.Cells[6].AddParagraph(Resources.SID_Trace);
            rowHeader.Cells[7].AddParagraph(Resources.SID_Additional_info);
        }

        private static void SetFooter(Section section)
        {
            Paragraph footer = new Paragraph();
            var reportNameInFooter = string.Format(Resources.SID_User_operations_log);
            var timestamp = $@"{DateTime.Now:g}";
            timestamp = timestamp.PadLeft(20, '\u00A0');
            var pageNumber = Resources.SID_Page_;
            pageNumber = pageNumber.PadLeft(90, '\u00A0');
            footer.AddFormattedText($@"Fibertest 2.0 (c)  {reportNameInFooter}.{timestamp}{pageNumber}");
            footer.AddPageField();
            footer.AddFormattedText(@" / ");
            footer.AddNumPagesField();
            footer.Format.Font.Size = 10;
            footer.Format.Alignment = ParagraphAlignment.Left;
            section.Footers.Primary.Add(footer);
            section.Footers.EvenPage.Add(footer.Clone());
        }
    }
}