using System;
using System.IO;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace Iit.Fibertest.Client
{
    public class OpticalEventsReportProvider
    {
        private readonly CurrentDatacenterParameters _server;
        private readonly CurrentUser _currentUser;
        private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;

        public OpticalEventsReportProvider(CurrentDatacenterParameters server, CurrentUser currentUser,
            OpticalEventsDoubleViewModel opticalEventsDoubleViewModel)
        {
            _server = server;
            _currentUser = currentUser;
            _opticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
        }

        public PdfDocument Create()
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

            LetsGetStarted(section);

            var table = CreateTable(section);
            FillInTable(table);

            PdfDocumentRenderer pdfDocumentRenderer =
                new PdfDocumentRenderer(true) { Document = doc };
            pdfDocumentRenderer.RenderDocument();

            return pdfDocumentRenderer.PdfDocument;
        }

        private void LetsGetStarted(Section section)
        {
            var headerFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\Reports\Header-landscape.png");
            var image = section.AddImage(headerFileName);
            image.LockAspectRatio = true;

            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText(Resources.SID_Current_optical_events_report, TextFormat.Bold);
            paragraph.Format.Font.Size = 20;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(1.0);

            var paragraph2 = section.AddParagraph();
            var software = string.Format(Resources.SID_software____0_, _server.DatacenterVersion);
            var server = string.Format(Resources.SID_Server_____0_____1_____2_, _server.ServerTitle, _server.ServerIp, software);
            paragraph2.AddFormattedText(server, TextFormat.Bold);
            paragraph2.Format.Font.Size = 14;
            paragraph2.Format.SpaceBefore = Unit.FromCentimeter(0.4);

            var paragraph3 = section.AddParagraph();
            var zone = string.Format(Resources.SID_Responsibility_zone___0_, _currentUser.ZoneTitle);
            paragraph3.AddFormattedText(zone, TextFormat.Bold);
            paragraph3.Format.Font.Size = 14;
            paragraph3.Format.SpaceBefore = Unit.FromCentimeter(0.4);
        }

        private void SetFooter(Section section)
        {
            Paragraph footer = new Paragraph();
            var reportNameInFooter = Resources.SID_Current_optical_events_report;
            var timestamp = $@"{DateTime.Now:g}";
            timestamp = timestamp.PadLeft(20, '\u00A0');
            var pageNumber = Resources.SID_Page_;
            pageNumber = pageNumber.PadLeft(120, '\u00A0');
            footer.AddFormattedText($@"Fibertest 2.0 (c) {reportNameInFooter}.{timestamp}{pageNumber}");
            footer.AddPageField();
            footer.AddFormattedText(@" / ");
            footer.AddNumPagesField();
            footer.Format.Font.Size = 10;
            footer.Format.Alignment = ParagraphAlignment.Left;
            section.Footers.Primary.Add(footer);
            section.Footers.EvenPage.Add(footer.Clone());
        }

        private void FillInTable(Table table)
        {
            var events = _opticalEventsDoubleViewModel.ActualOpticalEventsViewModel.Rows.ToList();
            foreach (var opticalEventModel in events)
            {
                var row = table.AddRow();
                row.VerticalAlignment = VerticalAlignment.Center;
                row.Cells[0].AddParagraph(opticalEventModel.SorFileId.ToString());
                var measurementTime = $@"{opticalEventModel.MeasurementTimestamp:G}";
                row.Cells[1].AddParagraph(measurementTime);
                var registrationTime = $@"{opticalEventModel.EventRegistrationTimestamp:G}";
                row.Cells[2].AddParagraph(registrationTime);
                row.Cells[3].AddParagraph($@"{opticalEventModel.TraceTitle}");
                row.Cells[4].AddParagraph($@"{opticalEventModel.TraceState.ToLocalizedString()}");
                row.Cells[5].AddParagraph($@"{opticalEventModel.EventStatus.GetLocalizedString()}");
                row.Cells[6].AddParagraph($@"{opticalEventModel.StatusChangedTimestamp}");
                row.Cells[7].AddParagraph($@"{opticalEventModel.StatusChangedByUser}");
            }
        }

        private Table CreateTable(Section section)
        {
            var gap = section.AddParagraph();
            gap.Format.SpaceBefore = Unit.FromCentimeter(0.4);

            var table = section.AddTable();
            table.Borders.Width = 0.25;

            table.AddColumn(@"1.8cm").Format.Alignment = ParagraphAlignment.Left;
            table.AddColumn(@"3.5cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"3.5cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"4.5cm").Format.Alignment = ParagraphAlignment.Left;
            table.AddColumn(@"3.2cm").Format.Alignment = ParagraphAlignment.Left;
            table.AddColumn(@"3cm").Format.Alignment = ParagraphAlignment.Left;
            table.AddColumn(@"3.5cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"4cm").Format.Alignment = ParagraphAlignment.Left;

            var headerRow1 = table.AddRow();
            headerRow1.HeadingFormat = true;
            headerRow1.Format.Alignment = ParagraphAlignment.Center;
            headerRow1.Format.Font.Bold = true;

            headerRow1.Cells[0].AddParagraph(Resources.SID_Event_Id);
            headerRow1.Cells[0].MergeDown = 1;
            headerRow1.Cells[0].VerticalAlignment = VerticalAlignment.Center;
            headerRow1.Cells[1].AddParagraph(Resources.SID_Time);
            headerRow1.Cells[1].MergeRight = 1;
            headerRow1.Cells[3].AddParagraph(Resources.SID_Trace);
            headerRow1.Cells[3].MergeDown = 1;
            headerRow1.Cells[3].VerticalAlignment = VerticalAlignment.Center;
            headerRow1.Cells[4].AddParagraph(Resources.SID_Trace_state);
            headerRow1.Cells[4].MergeDown = 1;
            headerRow1.Cells[4].VerticalAlignment = VerticalAlignment.Center;
            headerRow1.Cells[5].AddParagraph(Resources.SID_Event_status);
            headerRow1.Cells[5].MergeDown = 1;
            headerRow1.Cells[5].VerticalAlignment = VerticalAlignment.Center;
            headerRow1.Cells[6].AddParagraph(Resources.SID_Status_set);
            headerRow1.Cells[6].MergeRight = 1;

            var headerRow2 = table.AddRow();
            headerRow2.HeadingFormat = true;
            headerRow2.Format.Alignment = ParagraphAlignment.Center;
            headerRow2.Format.Font.Bold = true;

            headerRow2.Cells[1].AddParagraph(Resources.SID_measurement_finished);
            headerRow2.Cells[2].AddParagraph(Resources.SID_event_registered);
            headerRow2.Cells[6].AddParagraph(Resources.SID_Time);
            headerRow2.Cells[6].VerticalAlignment = VerticalAlignment.Center;
            headerRow2.Cells[7].AddParagraph(Resources.SID_User);
            headerRow2.Cells[7].VerticalAlignment = VerticalAlignment.Center;
            return table;
        }
    }
}