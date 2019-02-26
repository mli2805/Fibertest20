using System;
using System.IO;
using Iit.Fibertest.StringResources;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace Iit.Fibertest.Client
{
    public class AccidentsReportProvider
    {
        public PdfDocument Create()
        {
            Document doc = new Document();
            doc.DefaultPageSetup.LeftMargin = Unit.FromCentimeter(2);
            doc.DefaultPageSetup.RightMargin = Unit.FromCentimeter(1);
            doc.DefaultPageSetup.TopMargin = Unit.FromCentimeter(0.5);
            doc.DefaultPageSetup.BottomMargin = Unit.FromCentimeter(1.5);
            doc.DefaultPageSetup.FooterDistance = Unit.FromCentimeter(0.5);

            Section section = doc.AddSection();
            section.PageSetup.DifferentFirstPageHeaderFooter = false;

            SetFooter(section);

            LetsGetStarted(section);

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
            paragraph.AddFormattedText("Current accidents report", TextFormat.Bold);
            paragraph.Format.Font.Size = 20;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(1.4);
//            var paragraph2 = section.AddParagraph();
//            var software = string.Format(Resources.SID_software____0_, _server.DatacenterVersion);
//            var server = string.Format(Resources.SID_Server_____0_____1_____2_, _server.ServerTitle, _server.ServerIp, software);
//            paragraph2.AddFormattedText(server, TextFormat.Bold);
//            paragraph2.Format.Font.Size = 14;
//            paragraph2.Format.SpaceBefore = Unit.FromCentimeter(0.4);
        }

        private void SetFooter(Section section)
        {
            Paragraph footer = new Paragraph();
            var reportNameInFooter = "Current accidents report";
            footer.AddFormattedText($@"Fibertest 2.0 (c) {reportNameInFooter}. {DateTime.Today:d}");
            var pageNumber = Resources.SID_Page_;
            pageNumber = pageNumber.PadLeft(20, '\u00A0');
            footer.AddFormattedText(pageNumber);
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