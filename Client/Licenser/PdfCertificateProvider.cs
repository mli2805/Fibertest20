using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace Iit.Fibertest.Licenser
{
    public class PdfCertificateProvider
    {
        private LicenseInFileModel _licenseInFileModel;
        public PdfDocument Create(LicenseInFileModel licenseModel)
        {
            _licenseInFileModel = licenseModel;

            Document doc = new Document();
            Section section = doc.AddSection();
            doc.DefaultPageSetup.Orientation = Orientation.Portrait;
            doc.DefaultPageSetup.LeftMargin = Unit.FromCentimeter(2);
            doc.DefaultPageSetup.RightMargin = Unit.FromCentimeter(1);
            doc.DefaultPageSetup.TopMargin = Unit.FromCentimeter(0.5);
            doc.DefaultPageSetup.BottomMargin = Unit.FromCentimeter(1.5);
            doc.DefaultPageSetup.FooterDistance = Unit.FromCentimeter(0.5);

            AddCaption(section);
            AddMain(section);
            AddContent(section);
            if (_licenseInFileModel.IsMachineKeyRequired)
                AddSecurityAdminPasswordPage(section);

            PdfDocumentRenderer pdfDocumentRenderer =
                new PdfDocumentRenderer(true) { Document = doc };
            pdfDocumentRenderer.RenderDocument();

            return pdfDocumentRenderer.PdfDocument;
        }

        private void AddSecurityAdminPasswordPage(Section section)
        {
            section.AddPageBreak();
            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText(_licenseInFileModel.SecurityAdminPassword);
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(13.5);
            paragraph.Format.LeftIndent = Unit.FromCentimeter(7);
            paragraph.Format.Font.Size = 24;
            paragraph.Format.Font.Color = Colors.Gray;

            TextFrame tf = section.AddTextFrame();
            tf.RelativeHorizontal = RelativeHorizontal.Page;
            tf.RelativeVertical = RelativeVertical.Page;
            tf.Left = ShapePosition.Left;
            tf.Top = ShapePosition.Top;
            var background = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\Reports\PasswordBackground.png");
            tf.AddImage(background);

            TextFrame tf2 = section.AddTextFrame();
            tf2.RelativeHorizontal = RelativeHorizontal.Page;
            tf2.RelativeVertical = RelativeVertical.Page;
            tf2.Left = ShapePosition.Right;
            tf2.Top = Unit.FromCentimeter(28);
            tf2.Width = Unit.FromCentimeter(12);
            var paragraph2 = tf2.AddParagraph(Resources.SID_JS_Institute_of_Information_Technologies);
            paragraph2.Format.Font.Size = 14;
            paragraph2.Format.Font.Color = Colors.DarkGray;
        }

        private void AddCaption(Section section)
        {
            var headerFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\Reports\Header.png");
            section.AddImage(headerFileName);

            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText(Resources.SID_JS_Institute_of_Information_Technologies);
            paragraph.Format.Font.Size = 16;

            var paragraph2 = section.AddParagraph();
            paragraph2.AddFormattedText("220099, Республика Беларусь, г. Минск, ул. Казинца, д. 11а, офис А304.");
            paragraph2.Format.Font.Size = 12;
            paragraph2.Format.SpaceBefore = Unit.FromCentimeter(0.1);
        }

        private void AddMain(Section section)
        {
            var paragraph = section.AddParagraph();
            // paragraph.AddFormattedText("OPTICAL FIBER MONITORING SYSTEM SOFTWARE FIBERTEST 2.0");
            paragraph.AddFormattedText(Resources.SID_Optical_fiber_monitoring_system_software_FIBERTEST_2_0);
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.Format.Font.Size = 16;
            paragraph.Format.Font.Bold = true;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(1.4);

            var paragraph2 = section.AddParagraph();
            paragraph2.AddFormattedText(Resources.SID_License_number_);
            paragraph2.Format.Alignment = ParagraphAlignment.Center;
            paragraph2.Format.Font.Size = 16;
            paragraph2.Format.SpaceBefore = Unit.FromCentimeter(0.7);

            var paragraph3 = section.AddParagraph();
            paragraph3.AddFormattedText(_licenseInFileModel.LicenseKey);
            paragraph3.Format.Alignment = ParagraphAlignment.Center;
            paragraph3.Format.Font.Size = 20;
            paragraph3.Format.SpaceBefore = Unit.FromCentimeter(0.4);
            paragraph3.Format.SpaceAfter = Unit.FromCentimeter(1.4);
        }

        private void AddContent(Section section)
        {
            var licenseInFile = _licenseInFileModel.ToLicenseInFile();

            AddParam(section, Resources.SID_License_owner, _licenseInFileModel.Owner);
            AddCompleteParam(section, Resources.SID_Remote_testing_unit_count, licenseInFile.RtuCount);
            AddCompleteParam(section, Resources.SID_Client_stations, licenseInFile.ClientStationCount);
            AddCompleteParam(section, Resources.SID_Web_clients, licenseInFile.WebClientCount);
            AddCompleteParam(section, Resources.SID_SuperClients, licenseInFile.SuperClientStationCount);
            AddParam(section, Resources.SID_Creation_date, _licenseInFileModel.CreationDate.ToString("d"));

            AddDigitalKey(section, licenseInFile);
            AddSignature(section);
        }

        private void AddParam(Section section, string title, string value)
        {
            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText(title + ":  " + value);
            paragraph.Format.Font.Size = 12;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.4);
        }

        private void AddCompleteParam(Section section, string title, LicenseParameterInFile parameterInFile)
        {
            if (parameterInFile.Value <= 0) return;
            var paragraph = section.AddParagraph();
            string term;
            if (parameterInFile.Term == 999 && parameterInFile.IsTermInYears)
            {
                term = Resources.SID_with_no_limitation_by_time;
            }
            else
            {
                var yearInLocalLng = Thread.CurrentThread.CurrentUICulture.Equals(new CultureInfo("ru-RU"))
                    ? GetYearInRussian(parameterInFile.Value)
                    : "year(s)";
                var units = parameterInFile.IsTermInYears ? yearInLocalLng : Resources.SID_month_s_;
                term = string.Format(Resources.SID_for__0___1_, parameterInFile.Term, units);
            }
            var value = $"{parameterInFile.Value} ({term})";
            paragraph.AddFormattedText(title + ":  " + value);
            paragraph.Format.Font.Size = 12;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.4);
        }


        private string GetYearInRussian(int number)
        {
            if (number % 10 == 1 && number != 11)
            {
                return "год";
            }

            if ((number % 10 == 2 || number % 10 == 3 || number % 10 == 4) && (number < 12 || number > 14))
            {
                return "года";
            }

            return "лет";
        }

        private void AddDigitalKey(Section section, LicenseInFile licenseInFile)
        {
            var bytes = Cryptography.Encode(licenseInFile);

            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText("=== digital key starts ===");
            paragraph.Format.Font.Size = 8;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(1.4);

            var code = ByteArrayToString(bytes);
            var paragraph2 = section.AddParagraph();
            paragraph2.AddFormattedText(code);
            paragraph2.Format.Font.Size = 8;

            var paragraph3 = section.AddParagraph();
            paragraph3.AddFormattedText("=== digital key ends ===");
            paragraph3.Format.Font.Size = 8;
        }

        private static string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 3);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2} ", b);
            return hex.ToString().ToUpper();
        }

        private void AddSignature(Section section)
        {
            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText("     Директор     __________________________     Слесарчик М.В.");
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            paragraph.Format.LeftIndent = Unit.FromCentimeter(2);
            paragraph.Format.Font.Size = 12;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(2.4);

        }
    }
}
