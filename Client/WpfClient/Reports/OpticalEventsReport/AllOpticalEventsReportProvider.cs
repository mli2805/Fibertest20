using System;
using System.Collections.Generic;
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
    public class AllOpticalEventsReportProvider
    {
        private readonly CurrentDatacenterParameters _server;
        private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;
        private OpticalEventsReportModel _reportModel;

        public AllOpticalEventsReportProvider(CurrentDatacenterParameters server,
            OpticalEventsDoubleViewModel opticalEventsDoubleViewModel)
        {
            _server = server;
            _opticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
        }

        public PdfDocument Create(OpticalEventsReportModel reportModel)
        {
            _reportModel = reportModel;

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

            DrawBody(section);

            PdfDocumentRenderer pdfDocumentRenderer =
                new PdfDocumentRenderer(true) { Document = doc };
            pdfDocumentRenderer.RenderDocument();

            return pdfDocumentRenderer.PdfDocument;
        }
        private void SetFooter(Section section)
        {
            Paragraph footer = new Paragraph();
            var reportNameInFooter = string.Format(Resources.SID_Optical_events_report_for__0_d_____1_d_, _reportModel.DateFrom, _reportModel.DateTo);
            var timestamp = $@"{DateTime.Now:g}";
            timestamp = timestamp.PadLeft(20, '\u00A0');
            var pageNumber = Resources.SID_Page_;
            pageNumber = pageNumber.PadLeft(90, '\u00A0');
            footer.AddFormattedText($@"Fibertest 2.0 (c) {reportNameInFooter}.{timestamp}{pageNumber}");
            footer.AddPageField();
            footer.AddFormattedText(@" / ");
            footer.AddNumPagesField();
            footer.Format.Font.Size = 10;
            footer.Format.Alignment = ParagraphAlignment.Left;
            section.Footers.Primary.Add(footer);
            section.Footers.EvenPage.Add(footer.Clone());
        }

        private void LetsGetStarted(Section section)
        {
            var headerFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\Reports\Header-landscape.png");
            var image = section.AddImage(headerFileName);
            image.LockAspectRatio = true;

            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText(string.Format(Resources.SID_Optical_events_report_for__0_d_____1_d_, _reportModel.DateFrom, _reportModel.DateTo), TextFormat.Bold);
            paragraph.Format.Font.Size = 20;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(1.0);

            var paragraph2 = section.AddParagraph();
            var software = string.Format(Resources.SID_software____0_, _server.DatacenterVersion);
            var server = string.Format(Resources.SID_Server_____0_____1_____2_, _server.ServerTitle, _server.ServerIp, software);
            paragraph2.AddFormattedText(server, TextFormat.Bold);
            paragraph2.Format.Font.Size = 14;
            paragraph2.Format.SpaceBefore = Unit.FromCentimeter(0.4);

            var paragraph3 = section.AddParagraph();
            var zone = string.Format(Resources.SID_Responsibility_zone___0_, _reportModel.SelectedZone.Title);
            paragraph3.AddFormattedText(zone, TextFormat.Bold);
            paragraph3.Format.Font.Size = 14;
            paragraph3.Format.SpaceBefore = Unit.FromCentimeter(0.4);
        }

        private void DrawBody(Section section)
        {
            var gap = section.AddParagraph();
            gap.Format.SpaceBefore = Unit.FromCentimeter(0.4);

            DrawConsolidatedTable(section);
            if (_reportModel.IsDetailedReport)
                DrawOpticalEvents(section);
        }

        private void DrawConsolidatedTable(Section section)
        {
            var selectedStates = _reportModel.TraceStateSelectionViewModel.GetSelected();
            var data = AllEventsConsolidatedTableProvider.Create(_opticalEventsDoubleViewModel, _reportModel);
            var table = section.AddTable();
            table.Borders.Width = 0.25;

            table.AddColumn(@"3.5cm").Format.Alignment = ParagraphAlignment.Center;
            foreach (var _ in selectedStates)
                table.AddColumn(@"3.5cm").Format.Alignment = ParagraphAlignment.Center;

            var rowHeader = table.AddRow();
            rowHeader.VerticalAlignment = VerticalAlignment.Center;
            rowHeader.TopPadding = Unit.FromCentimeter(0.1);
            rowHeader.BottomPadding = Unit.FromCentimeter(0.1);
            rowHeader.Format.Font.Bold = true;
            for (int i = 0; i < selectedStates.Count; i++)
                rowHeader.Cells[i + 1].AddParagraph(selectedStates[i].ToLocalizedString());

            foreach (var list in data)
            {
                var row = table.AddRow();
                row.HeightRule = RowHeightRule.Exactly;
                row.Height = Unit.FromCentimeter(0.6);
                row.VerticalAlignment = VerticalAlignment.Center;
                for (int i = 0; i < list.Count; i++)
                    row.Cells[i].AddParagraph(list[i]);
            }
        }

        private Dictionary<int, DateTime> _closingTimes;
        private void DrawOpticalEvents(Section section)
        {
            _closingTimes = ClosedEventsProvider.Calculate(_opticalEventsDoubleViewModel);
            foreach (var eventStatus in EventStatusExt.EventStatusesInRightOrder)
            {
                if (_reportModel.EventStatusViewModel.GetSelected().Contains(eventStatus))
                    DrawOpticalEventsWithStatus(section, eventStatus);
            }
        }

        private void DrawOpticalEventsWithStatus(Section section, EventStatus eventStatus)
        {
            foreach (var state in _reportModel.TraceStateSelectionViewModel.GetSelected())
            {
                var events = _opticalEventsDoubleViewModel.AllOpticalEventsViewModel.
                    Rows.Where(r => r.EventStatus == eventStatus && r.TraceState == state).ToList();
                if (events.Any())
                    DrawOpticalEventsWithStatusAndState(section, events);
            }
        }

        private void DrawOpticalEventsWithStatusAndState(Section section, List<OpticalEventModel> events)
        {
            var gap = section.AddParagraph();
            gap.Format.SpaceBefore = Unit.FromCentimeter(0.4);

            var ev = events.First();
            var caption = section.AddParagraph($@"{ev.EventStatus.GetLocalizedString()} / {ev.TraceState.ToLocalizedString()} ({events.Count})");
            caption.Format.Font.Bold = true;

            if (!_reportModel.IsAccidentPlaceShown)
                DrawOpticalEventsTable(section, events);
            else
                DrawOpticalEventsTableWithAccidentPlaces(section, events);
        }

        private void DrawOpticalEventsTable(Section section, List<OpticalEventModel> events)
        {
            var table = DrawOpticalEventTableHeader(section);
            foreach (var opticalEventModel in events)
            {
                DrawOpticalEventRow(table, opticalEventModel);
            }
        }

        private void DrawOpticalEventsTableWithAccidentPlaces(Section section, List<OpticalEventModel> events)
        {
            foreach (var opticalEventModel in events)
            {
                var table = DrawOpticalEventTableHeader(section);
                DrawOpticalEventRow(table, opticalEventModel);
            }
        }

        private Table DrawOpticalEventTableHeader(Section section)
        {
            var gap = section.AddParagraph();
            gap.Format.SpaceBefore = Unit.FromCentimeter(0.2);
            var table = section.AddTable();
            table.Borders.Width = 0.25;

            table.AddColumn(@"1.8cm").Format.Alignment = ParagraphAlignment.Left;
            table.AddColumn(@"3.5cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"3.5cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"3.5cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"4.5cm").Format.Alignment = ParagraphAlignment.Left;

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
            headerRow1.Cells[1].MergeRight = 2;
            headerRow1.Cells[4].AddParagraph(Resources.SID_Trace);
            headerRow1.Cells[4].MergeDown = 1;
            headerRow1.Cells[4].VerticalAlignment = VerticalAlignment.Center;

            headerRow1.Cells[5].AddParagraph(Resources.SID_Status_set);
            headerRow1.Cells[5].MergeRight = 1;

            var headerRow2 = table.AddRow();
            headerRow2.HeadingFormat = true;
            headerRow2.Format.Alignment = ParagraphAlignment.Center;
            headerRow2.Format.Font.Bold = true;
            headerRow2.VerticalAlignment = VerticalAlignment.Center;

            headerRow2.Cells[1].AddParagraph(Resources.SID_measurement_finished);
            headerRow2.Cells[2].AddParagraph(Resources.SID_event_registered);
            headerRow2.Cells[3].AddParagraph(Resources.SID_Turned_to_OK);
            headerRow2.Cells[5].AddParagraph(Resources.SID_Time);
            headerRow2.Cells[5].VerticalAlignment = VerticalAlignment.Center;
            headerRow2.Cells[6].AddParagraph(Resources.SID_User);
            headerRow2.Cells[6].VerticalAlignment = VerticalAlignment.Center;
            return table;
        }

        private void DrawOpticalEventRow(Table table, OpticalEventModel opticalEventModel)
        {
            var row = table.AddRow();
            row.HeightRule = RowHeightRule.Exactly;
            row.Height = Unit.FromCentimeter(0.8);
            row.VerticalAlignment = VerticalAlignment.Center;
            row.Cells[0].AddParagraph(opticalEventModel.SorFileId.ToString());
            var measurementTime = $@"{opticalEventModel.MeasurementTimestamp:G}";
            row.Cells[1].AddParagraph(measurementTime);
            var registrationTime = $@"{opticalEventModel.EventRegistrationTimestamp:G}";
            row.Cells[2].AddParagraph(registrationTime);

            if (_closingTimes.ContainsKey(opticalEventModel.SorFileId))
            {
                var closingTime = $@"{_closingTimes[opticalEventModel.SorFileId]:G}";
                row.Cells[3].AddParagraph(closingTime);
            }
            row.Cells[4].AddParagraph($@"{opticalEventModel.TraceTitle}");
            row.Cells[5].AddParagraph($@"{opticalEventModel.StatusChangedTimestamp}");
            row.Cells[6].AddParagraph($@"{opticalEventModel.StatusChangedByUser}");

            if (!string.IsNullOrEmpty(opticalEventModel.Comment))
            {
                var commentRow = table.AddRow();
                commentRow.Cells[0].MergeRight = 6;
                commentRow.Cells[0].AddParagraph(opticalEventModel.Comment);
            }
        }
    }

    public static class ClosedEventsProvider
    {
        public static Dictionary<int, DateTime> Calculate(OpticalEventsDoubleViewModel opticalEventsDoubleViewModel)
        {
            var allAccidents = opticalEventsDoubleViewModel.AllOpticalEventsViewModel.Rows.
                    Where(r => r.EventStatus > EventStatus.EventButNotAnAccident).ToList();
            var result = new Dictionary<int, DateTime>();
            foreach (var opticalEventModel in allAccidents)
            {
                var okEvent = opticalEventsDoubleViewModel.AllOpticalEventsViewModel.Rows.FirstOrDefault(e =>
                    e.TraceId == opticalEventModel.TraceId && e.TraceState == FiberState.Ok
                      && e.MeasurementTimestamp >= opticalEventModel.MeasurementTimestamp);
                if (okEvent != null)
                    result.Add(opticalEventModel.SorFileId, okEvent.MeasurementTimestamp);
            }
            return result;
        }
    }
}