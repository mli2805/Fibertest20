using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace Iit.Fibertest.Client
{
    public class AllOpticalEventsReportProvider
    {
        private readonly CurrentDatacenterParameters _server;
        private readonly Model _readModel;
        private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;
        private readonly AccidentLineModelFactory _accidentLineModelFactory;
        private OpticalEventsReportModel _reportModel;

        public AllOpticalEventsReportProvider(CurrentDatacenterParameters server,
            Model readModel, OpticalEventsDoubleViewModel opticalEventsDoubleViewModel,
            AccidentLineModelFactory accidentLineModelFactory)
        {
            _server = server;
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

            SetFooter(section);

            LetsGetStarted(section);

            DrawBody(section, _reportModel.IsAccidentPlaceShown);

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

        private void DrawBody(Section section, bool isAccidentPlaceShown)
        {
            DrawConsolidatedTable(section);
            DrawOpticalEvents(section, isAccidentPlaceShown);
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
            for (int i = 0; i < selectedStates.Count; i++)
                rowHeader.Cells[i + 1].AddParagraph(selectedStates[i].ToLocalizedString());

            foreach (var list in data)
            {
                var row = table.AddRow();
                for (int i = 0; i < list.Count; i++)
                    row.Cells[i].AddParagraph(list[i]);
            }
        }

        private void DrawOpticalEvents(Section section, bool isAccidentPlaceShown)
        {

        }
    }

    public static class AllEventsConsolidatedTableProvider
    {
        public static List<List<string>> Create(OpticalEventsDoubleViewModel opticalEventsDoubleViewModel, OpticalEventsReportModel reportModel)
        {
            var result = new List<List<string>>();
            var traceStates = new List<string> { "" };
            foreach (var state in reportModel.TraceStateSelectionViewModel.GetSelected())
                traceStates.Add(state.ToLocalizedString());
            result.Add(traceStates);

            var data = Calculate(opticalEventsDoubleViewModel, reportModel);
            foreach (var pair in data)
            {
                var statusLine = new List<string>() { pair.Key.GetLocalizedString() };
                foreach (var state in reportModel.TraceStateSelectionViewModel.GetSelected())
                {
                    statusLine.Add(pair.Value.ContainsKey(state) ? pair.Value[state].ToString() : @"0");
                }
                result.Add(statusLine);
            }
            return result;
        }

        private static Dictionary<EventStatus, Dictionary<FiberState, int>>
            Calculate(OpticalEventsDoubleViewModel opticalEventsDoubleViewModel, OpticalEventsReportModel reportModel)
        {
            var result = new Dictionary<EventStatus, Dictionary<FiberState, int>>();
            foreach (var meas in opticalEventsDoubleViewModel.AllOpticalEventsViewModel.Rows.Where(r => r.IsEventForReport(reportModel)))
            {
                if (result.ContainsKey(meas.EventStatus))
                {
                    if (result[meas.EventStatus].ContainsKey(meas.TraceState))
                    {
                        result[meas.EventStatus][meas.TraceState]++;
                    }
                    else
                    {
                        result[meas.EventStatus].Add(meas.TraceState, 1);
                    }
                }
                else
                {
                    result.Add(meas.EventStatus, new Dictionary<FiberState, int> { { meas.TraceState, 1 } });
                }
            }
            return result;
        }

        private static bool IsEventForReport(this OpticalEventModel opticalEventModel, OpticalEventsReportModel reportModel)
        {
            if (opticalEventModel.MeasurementTimestamp.Date < reportModel.DateFrom.Date) return false;
            if (opticalEventModel.MeasurementTimestamp.Date > reportModel.DateTo.Date) return false;
            if (!reportModel.EventStatusViewModel.GetSelected().Contains(opticalEventModel.EventStatus)) return false;
            return reportModel.TraceStateSelectionViewModel.GetSelected().Contains(opticalEventModel.TraceState);
        }
    }
}