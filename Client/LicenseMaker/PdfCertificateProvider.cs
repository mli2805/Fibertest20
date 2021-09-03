using Iit.Fibertest.StringResources;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace LicenseMaker
{
    public class PdfCertificateProvider
    {
        private LicenseInFileModel _licenseInFileModel;
        public PdfDocument Create(LicenseInFileModel licenseModel)
        {
            _licenseInFileModel = licenseModel;

            Document doc = new Document();

            Section section = doc.AddSection();



            PdfDocumentRenderer pdfDocumentRenderer =
                new PdfDocumentRenderer(true) { Document = doc };
            pdfDocumentRenderer.RenderDocument();

            return pdfDocumentRenderer.PdfDocument;
        }

        private void AddContent(Section section)
        {
            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText(Resources.SID_Monitoring_system_components, TextFormat.Bold);
            paragraph.Format.Font.Size = 20;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(1.4);

            var paragraph2 = section.AddParagraph();
            var software = string.Format(Resources.SID_software____0_, _licenseInFileModel.LicenseKey);
            paragraph2.AddFormattedText(software, TextFormat.Bold);
            paragraph2.Format.Font.Size = 14;
            paragraph2.Format.SpaceBefore = Unit.FromCentimeter(0.4);

        }
    }
}
