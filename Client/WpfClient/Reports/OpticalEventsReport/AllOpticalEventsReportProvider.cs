using System;
using System.IO;
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
            var gap = section.AddParagraph();
            gap.Format.SpaceBefore = Unit.FromCentimeter(0.4);

            DrawConsolidatedTable(section);
            if (_reportModel.IsDetailedReport)
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

        private void DrawOpticalEvents(Section section, bool isAccidentPlaceShown)
        {

        }
    }
}