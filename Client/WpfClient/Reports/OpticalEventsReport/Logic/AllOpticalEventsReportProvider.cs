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
        private readonly CurrentUser _currentUser;
        private readonly CurrentGis _currentGis;
        private readonly Model _readModel;
        private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;
        private readonly AccidentLineModelFactory _accidentLineModelFactory;
        private OpticalEventsReportModel _reportModel;

        private List<OpticalEventModel> _events;

        public AllOpticalEventsReportProvider(CurrentDatacenterParameters server, CurrentUser currentUser,
            CurrentGis currentGis, Model readModel,
            OpticalEventsDoubleViewModel opticalEventsDoubleViewModel, AccidentLineModelFactory accidentLineModelFactory)
        {
            _server = server;
            _currentUser = currentUser;
            _currentGis = currentGis;
            _readModel = readModel;
            _opticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
            _accidentLineModelFactory = accidentLineModelFactory;
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

            var reportNameInFooter = string.Format(Resources.SID_Optical_events_report_for__0_d_____1_d_, _reportModel.DateFrom, _reportModel.DateTo);
            section.SetLandscapeFooter(reportNameInFooter);

            LetsGetStarted(section);

            _events = _currentUser.ZoneId == _reportModel.SelectedZone.ZoneId
                ? _opticalEventsDoubleViewModel.AllOpticalEventsViewModel.Rows.ToList()
                : FilterZoneEvents();
            DrawBody(section);

            PdfDocumentRenderer pdfDocumentRenderer =
                new PdfDocumentRenderer(true) { Document = doc };
            pdfDocumentRenderer.RenderDocument();

            return pdfDocumentRenderer.PdfDocument;
        }

        private List<OpticalEventModel> FilterZoneEvents()
        {
            var result = new List<OpticalEventModel>();
            foreach (var opticalEventModel in _opticalEventsDoubleViewModel.AllOpticalEventsViewModel.Rows)
            {
                var trace = _readModel.Traces.First(t => t.TraceId == opticalEventModel.TraceId);
                if (trace.ZoneIds.Contains(_reportModel.SelectedZone.ZoneId))
                    result.Add(opticalEventModel);
            }
            return result;
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
            var selectedStates = _reportModel.TraceStateSelectionViewModel.GetCheckedStates();
            var data = OpticalEventsReportFunctions.Create(_events, _reportModel);
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
            _closingTimes = _events.GetAccidentsClosingTimes();
            var checkedStatuses = _reportModel.EventStatusViewModel.GetCheckedStatuses();
            foreach (var eventStatus in EventStatusExt.EventStatusesInRightOrder.Where(eventStatus => checkedStatuses.Contains(eventStatus)))
            {
                DrawOpticalEventsWithStatus(section, eventStatus);
            }
        }

        private void DrawOpticalEventsWithStatus(Section section, EventStatus eventStatus)
        {
            foreach (var state in _reportModel.TraceStateSelectionViewModel.GetCheckedStates())
            {
                var events = _events
                        .Where(r => r.EventStatus == eventStatus
                                && r.TraceState == state
                                && (r.MeasurementTimestamp.Date >= _reportModel.DateFrom && r.MeasurementTimestamp.Date <= _reportModel.DateTo))
                        .OrderByDescending(e => e.EventRegistrationTimestamp)
                        .ToList();
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
                AccidentPlaceReportProvider.DrawAccidents(opticalEventModel.Accidents, section,
                    _accidentLineModelFactory, _currentGis.IsGisOn, _currentGis.GpsInputMode);
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
            row.HeightRule = RowHeightRule.Auto;
            row.Height = Unit.FromCentimeter(0.8);
            row.VerticalAlignment = VerticalAlignment.Center;
            row.Cells[0].AddParagraph(opticalEventModel.SorFileId.ToString());
            var measurementTime = $@"{opticalEventModel.MeasurementTimestamp:G}";
            row.Cells[1].AddParagraph(measurementTime);
            var registrationTime = $@"{opticalEventModel.EventRegistrationTimestamp:G}";
            row.Cells[2].AddParagraph(registrationTime);

            if (_closingTimes.TryGetValue(opticalEventModel.SorFileId, out var time))
            {
                var closingTime = $@"{time:G}";
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
}