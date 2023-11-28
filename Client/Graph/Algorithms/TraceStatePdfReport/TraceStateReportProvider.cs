using System;
using System.IO;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace Iit.Fibertest.Graph
{
    public class TraceStateReportProvider
    {
        private TraceReportModel _traceReportModel;
        private CurrentDatacenterParameters _server;
        public PdfDocument Create(TraceReportModel traceReportModel, CurrentDatacenterParameters server)
        {
            _traceReportModel = traceReportModel;
            _server = server;

            Document doc = new Document();
            doc.DefaultPageSetup.Orientation = Orientation.Portrait;
            doc.DefaultPageSetup.LeftMargin = Unit.FromCentimeter(2);
            doc.DefaultPageSetup.RightMargin = Unit.FromCentimeter(1);
            doc.DefaultPageSetup.TopMargin = Unit.FromCentimeter(0.5);
            doc.DefaultPageSetup.BottomMargin = Unit.FromCentimeter(1.5);
            doc.DefaultPageSetup.FooterDistance = Unit.FromCentimeter(0.5);

            Section section = doc.AddSection();
            section.PageSetup.DifferentFirstPageHeaderFooter = false;

            SetFooter(section);
            LetsGetStarted(section);
            DrawTextTable(section);
            AccidentPlaceReportProvider.DrawAccidents(_traceReportModel.Accidents, section);

            PdfDocumentRenderer pdfDocumentRenderer =
                new PdfDocumentRenderer(true) { Document = doc };
            pdfDocumentRenderer.RenderDocument();
            return pdfDocumentRenderer.PdfDocument;
        }

        private void LetsGetStarted(Section section)
        {
            var headerFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\Reports\Header.png");
            var image = section.AddImage(headerFileName);
            image.LockAspectRatio = true;

            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText(Resources.SID_Trace_State_Report, TextFormat.Bold);
            paragraph.Format.Font.Size = 20;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(1.0);

            var paragraph2 = section.AddParagraph();
            var title = !string.IsNullOrEmpty(_server.ServerTitle) ? string.Format(Resources.SID_ServerTitle, _server.ServerTitle) : "";
            var software = "";
            //   var software = string.Format(Resources.SID_software____0_, _server.DatacenterVersion);
            var server = string.Format(Resources.SID_Server_____0_____1_____2_, title, _server.ServerIp, software);
            paragraph2.AddFormattedText(server, TextFormat.Bold);
            paragraph2.Format.Font.Size = 14;
            paragraph2.Format.SpaceBefore = Unit.FromCentimeter(0.4);
        }

        private void DrawTextTable(Section section)
        {
            var gap = section.AddParagraph();
            gap.Format.SpaceBefore = Unit.FromCentimeter(0.4);

            var table = section.AddTable();
            table.Borders.Visible = false;
            table.Format.Font.Size = 14;
            table.AddColumn(@"3.5cm").Format.Alignment = ParagraphAlignment.Right;
            table.AddColumn(@"0.5cm");
            table.AddColumn(@"14cm").Format.Alignment = ParagraphAlignment.Left;


            var row = table.AddRow();
            row.HeightRule = RowHeightRule.AtLeast; row.Height = Unit.FromCentimeter(1);
            row.VerticalAlignment = VerticalAlignment.Center;
            row.Cells[0].AddParagraph(Resources.SID_Trace);
            row.Cells[2].AddParagraph($@"{_traceReportModel.TraceTitle}");

            var rowBetween = table.AddRow();
            rowBetween.HeightRule = RowHeightRule.Exactly; rowBetween.Height = Unit.FromMillimeter(4);

            row = table.AddRow();
            row.VerticalAlignment = VerticalAlignment.Center;
            row.Cells[0].AddParagraph(Resources.SID_Trace_state);
            var p = row.Cells[2].AddParagraph($@"{_traceReportModel.TraceState}");
            p.Format.Font.Bold = true;

            rowBetween = table.AddRow();
            rowBetween.HeightRule = RowHeightRule.Exactly; rowBetween.Height = Unit.FromMillimeter(4);

            row = table.AddRow();
            row.Cells[0].AddParagraph(@"RTU");
            row.Cells[2].AddParagraph(_traceReportModel.RtuTitle);
            //  row.Cells[2].AddParagraph(string.Format(Resources.SID__0_____software__1_, _traceReportModel.RtuTitle, _traceReportModel.RtuSoftwareVersion));

            rowBetween = table.AddRow();
            rowBetween.HeightRule = RowHeightRule.Exactly; rowBetween.Height = Unit.FromMillimeter(4);

            row = table.AddRow();
            row.Cells[0].AddParagraph(Resources.SID_Port);
            row.Cells[2].AddParagraph($@"{_traceReportModel.PortTitle}");

            rowBetween = table.AddRow();
            rowBetween.HeightRule = RowHeightRule.Exactly; rowBetween.Height = Unit.FromMillimeter(4);

            row = table.AddRow();
            row.VerticalAlignment = VerticalAlignment.Center;
            row.Cells[0].AddParagraph(Resources.SID_Measurement_time);
            row.Cells[2].AddParagraph($@"{_traceReportModel.MeasurementTimestamp}");

            rowBetween = table.AddRow();
            rowBetween.HeightRule = RowHeightRule.Exactly; rowBetween.Height = Unit.FromMillimeter(4);

            row = table.AddRow();
            row.VerticalAlignment = VerticalAlignment.Center;
            row.Cells[0].AddParagraph(Resources.SID_Event_registration_time);
            row.Cells[2].AddParagraph($@"{_traceReportModel.RegistrationTimestamp}");
        }


        private void SetFooter(Section section)
        {
            Paragraph footer = new Paragraph();
            var reportNameInFooter = Resources.SID_Trace_State_Report;
            var timestamp = $@"{DateTime.Now:g}";
            timestamp = timestamp.PadLeft(20, '\u00A0');
            footer.AddFormattedText($@"Fibertest 2.0 (c) {reportNameInFooter}.{timestamp}");

            footer.Format.Font.Size = 10;
            footer.Format.Alignment = ParagraphAlignment.Left;
            section.Footers.Primary.Add(footer);
            section.Footers.EvenPage.Add(footer.Clone());
        }

    }
}