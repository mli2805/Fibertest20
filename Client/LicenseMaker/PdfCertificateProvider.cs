﻿using System.Globalization;
using System.Text;
using System.Threading;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace Iit.Fibertest.LicenseMaker
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

            AddContent(section);

            PdfDocumentRenderer pdfDocumentRenderer =
                new PdfDocumentRenderer(true) { Document = doc };
            pdfDocumentRenderer.RenderDocument();

            return pdfDocumentRenderer.PdfDocument;
        }

        private void AddContent(Section section)
        {
            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText(Resources.SID_Institute_of_Information_Technologies);
            paragraph.Format.Alignment = ParagraphAlignment.Center;
            paragraph.Format.Font.Size = 16;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.4);

            var paragraph2 = section.AddParagraph();
            paragraph2.AddFormattedText(Resources.SID_License_Key);
            paragraph2.Format.Font.Size = 20;
            paragraph2.Format.SpaceBefore = Unit.FromCentimeter(1.4);

            var paragraph3 = section.AddParagraph();
            paragraph3.AddFormattedText(_licenseInFileModel.LicenseKey);
            paragraph3.Format.Alignment = ParagraphAlignment.Center;
            paragraph3.Format.Font.Size = 20;
            paragraph3.Format.SpaceBefore = Unit.FromCentimeter(0.4);
            paragraph3.Format.SpaceAfter = Unit.FromCentimeter(1.4);

            var licenseInFile = _licenseInFileModel.ToLicenseInFile();

            AddParam(section, Resources.SID_License_owner, _licenseInFileModel.Owner);
            AddCompleteParam(section, Resources.SID_Rtu_count, licenseInFile.RtuCount);
            AddCompleteParam(section, Resources.SID_Client_stations, licenseInFile.ClientStationCount);
            AddCompleteParam(section, Resources.SID_Web_clients, licenseInFile.WebClientCount);
            AddCompleteParam(section, Resources.SID_SuperClients, licenseInFile.SuperClientStationCount);
            AddParam(section, Resources.SID_Creation_date, _licenseInFileModel.CreationDate.ToString("d"));

            AddDigitalFootprint(section, licenseInFile);
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

        private void AddDigitalFootprint(Section section, LicenseInFile licenseInFile)
        {
            var bytes = new LicenseManager().Encode(licenseInFile);

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
    }
}
