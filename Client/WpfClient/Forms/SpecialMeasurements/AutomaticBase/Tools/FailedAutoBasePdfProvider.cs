using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Graph;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace Iit.Fibertest.Client
{
    public class FailedAutoBasePdfProvider
    {
        public PdfDocument Create(Rtu rtu, List<MeasurementCompletedEventArgs> measurements)
        {
            Document doc = new Document();
            doc.DefaultPageSetup.Orientation = Orientation.Portrait;
            doc.DefaultPageSetup.LeftMargin = Unit.FromCentimeter(2);
            doc.DefaultPageSetup.RightMargin = Unit.FromCentimeter(1);
            doc.DefaultPageSetup.TopMargin = Unit.FromCentimeter(0.5);
            doc.DefaultPageSetup.BottomMargin = Unit.FromCentimeter(1.5);
            doc.DefaultPageSetup.FooterDistance = Unit.FromCentimeter(0.5);

            Section section = doc.AddSection();
            section.PageSetup.DifferentFirstPageHeaderFooter = false;

            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText($@"RTU {rtu.Title}", TextFormat.Bold);
            paragraph.Format.Font.Size = 16;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(1.4);

            foreach (var measurement in measurements)
            {
                OneFailedMeasurement(section, measurement);
            }

            PdfDocumentRenderer pdfDocumentRenderer =
                new PdfDocumentRenderer(true) { Document = doc };
            pdfDocumentRenderer.RenderDocument();

            return pdfDocumentRenderer.PdfDocument;
        }

        public void OneFailedMeasurement(Section section, MeasurementCompletedEventArgs measurement)
        {
            var traceParagraph = section.AddParagraph();
            traceParagraph.Format.Font.Size = 12;
            traceParagraph.Format.SpaceBefore = Unit.FromCentimeter(0.2);
            traceParagraph.AddFormattedText($@"{StringResources.Resources.SID_Trace}: {measurement.Trace.Title}", TextFormat.Bold);

            var statusParagraph = section.AddParagraph();
            statusParagraph.Format.Font.Size = 11;
            statusParagraph.Format.SpaceBefore = Unit.FromCentimeter(0.2);
            statusParagraph.Format.LeftIndent = Unit.FromCentimeter(1);
            statusParagraph.AddFormattedText($@"{measurement.CompletedStatus.GetLocalizedString()}");

            foreach (var line in measurement.Lines.Where(l => l != null))
            {
                var lineParagraph = section.AddParagraph();
                lineParagraph.Format.Font.Size = 10;
                lineParagraph.Format.SpaceBefore = Unit.FromCentimeter(0.2);
                lineParagraph.Format.LeftIndent = Unit.FromCentimeter(1);
                lineParagraph.AddFormattedText(line);
            }

        }
    }
}
