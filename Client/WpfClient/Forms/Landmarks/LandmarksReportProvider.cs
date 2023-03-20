using System.Collections.Generic;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace Iit.Fibertest.Client
{
    public static class LandmarksReportProvider
    {
        public static PdfDocument Create(List<Landmark> list, string traceTitle, GpsInputMode mode)
        {
            Document doc = new Document();
            doc.DefaultPageSetup.Orientation = Orientation.Landscape;
            doc.DefaultPageSetup.LeftMargin = Unit.FromCentimeter(1);
            doc.DefaultPageSetup.RightMargin = Unit.FromCentimeter(1);
            doc.DefaultPageSetup.TopMargin = Unit.FromCentimeter(0.5);
            doc.DefaultPageSetup.BottomMargin = Unit.FromCentimeter(1.5);
            doc.DefaultPageSetup.FooterDistance = Unit.FromCentimeter(0.5);

            Section section = doc.AddSection();
            section.PageSetup.DifferentFirstPageHeaderFooter = false;
            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText(Resources.SID_Landmarks, TextFormat.NotBold);
            paragraph.Format.Font.Size = 14;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.4);
            var paragraph2 = section.AddParagraph();
            paragraph2.AddFormattedText(traceTitle, TextFormat.NotBold);
            paragraph2.Format.Font.Size = 14;
            paragraph2.Format.SpaceBefore = Unit.FromCentimeter(0.4);
            paragraph2.Format.SpaceAfter = Unit.FromCentimeter(0.4);

            DrawTable(list, section, mode);

            PdfDocumentRenderer pdfDocumentRenderer =
                new PdfDocumentRenderer(true) { Document = doc };
            pdfDocumentRenderer.RenderDocument();

            return pdfDocumentRenderer.PdfDocument;
        }

        private static void DrawTable(List<Landmark> list, Section section, GpsInputMode mode)
        {
            var table = section.AddTable();
            table.Borders.Width = 0.25;
            SetColumns(table);
            FillInTableHeader(table);
          
            FillInTable(list, mode, table);
        }

        private static void FillInTable(List<Landmark> list, GpsInputMode mode, Table table)
        {
            foreach (var lm in list)
            {
                var lmRow = new LandmarkRow().FromLandmark(lm, mode);

                var row = table.AddRow();
                row.Height = Unit.FromCentimeter(0.6);
                row.HeightRule = RowHeightRule.AtLeast;
                row.VerticalAlignment = VerticalAlignment.Center;
                row.Cells[0].AddParagraph(lmRow.Number.ToString());
                row.Cells[1].AddParagraph(lmRow.NodeTitle ?? "");
                row.Cells[2].AddParagraph(lmRow.EquipmentType);
                row.Cells[3].AddParagraph(lmRow.EquipmentTitle ?? "");

                row.Cells[4].AddParagraph($@"{lmRow.CableReserves}");
                row.Cells[5].AddParagraph($@"{lmRow.GpsDistance}");
                row.Cells[6].AddParagraph($@"{lmRow.GpsSection}");
                row.Cells[6].Shading.Color = lmRow.IsUserInput 
                    ? Color.FromArgb(0xff, 0xd3, 0xd3, 0xd3)
                    : Color.Empty;

                row.Cells[7].AddParagraph($@"{lmRow.OpticalDistance}");
                row.Cells[8].AddParagraph($@"{lmRow.OpticalSection}");

                row.Cells[9].AddParagraph(lmRow.EventNumber);
                row.Cells[10].AddParagraph(lmRow.GpsCoors);
            }
        }

        private static void FillInTableHeader(Table table)
        {
            var rowHeader = table.AddRow();
            rowHeader.HeadingFormat = true;
            rowHeader.VerticalAlignment = VerticalAlignment.Center;
            rowHeader.TopPadding = Unit.FromCentimeter(0.1);
            rowHeader.BottomPadding = Unit.FromCentimeter(0.1);
            rowHeader.Format.Font.Bold = true;
            rowHeader.Cells[0].AddParagraph(@"No");
            rowHeader.Cells[1].AddParagraph(Resources.SID_Node);
            rowHeader.Cells[2].AddParagraph(Resources.SID_Type);
            rowHeader.Cells[3].AddParagraph(Resources.SID_Equipm__title);

            rowHeader.Cells[4].AddParagraph(Resources.SID_Cable_reserve_m);
            rowHeader.Cells[5].AddParagraph(Resources.SID_Gps_km);
            rowHeader.Cells[6].AddParagraph(Resources.SID_Gps_section_km);
            rowHeader.Cells[7].AddParagraph(Resources.SID_Optical_km);
            rowHeader.Cells[8].AddParagraph(Resources.SID_Opt_section_km);


            rowHeader.Cells[9].AddParagraph(Resources.SID_Event);
            rowHeader.Cells[10].AddParagraph(Resources.SID_GPS_coordinates);
        }

        private static void SetColumns(Table table)
        {
            table.AddColumn(@"1cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"4.5cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"3.5cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"4cm").Format.Alignment = ParagraphAlignment.Center;

            table.AddColumn(@"1.5cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"1.5cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"1.5cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"1.5cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"1.5cm").Format.Alignment = ParagraphAlignment.Center;


            table.AddColumn(@"2.0cm").Format.Alignment = ParagraphAlignment.Center;
            table.AddColumn(@"5cm").Format.Alignment = ParagraphAlignment.Center;
        }
    }
}