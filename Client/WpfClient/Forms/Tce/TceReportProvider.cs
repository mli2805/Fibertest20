using System;
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
    public class TceReportProvider
    {
        private readonly Model _readModel;

        public TceReportProvider(Model readModel)
        {
            _readModel = readModel;
        }

        public PdfDocument Create(TceS tce)
        {
            Document doc = new Document();
            doc.DefaultPageSetup.Orientation = Orientation.Landscape;
            doc.DefaultPageSetup.LeftMargin = Unit.FromCentimeter(2);
            doc.DefaultPageSetup.RightMargin = Unit.FromCentimeter(1);
            doc.DefaultPageSetup.TopMargin = Unit.FromCentimeter(0.5);
            doc.DefaultPageSetup.BottomMargin = Unit.FromCentimeter(1.5);
            doc.DefaultPageSetup.FooterDistance = Unit.FromCentimeter(0.5);

            Section section = doc.AddSection();
            section.PageSetup.DifferentFirstPageHeaderFooter = false;

            section.SetLandscapeFooter(tce.Title);
            LetsGetStarted(section, tce);

            foreach (var tceSlot in tce.Slots)
            {
                FillOneSlot(section, tce, tceSlot);
            }

            PdfDocumentRenderer pdfDocumentRenderer =
                new PdfDocumentRenderer(true) { Document = doc };
            pdfDocumentRenderer.RenderDocument();

            return pdfDocumentRenderer.PdfDocument;
        }

        private void FillOneSlot(Section section, TceS tce, TceSlot tceSlot)
        {
            var line = $@"{Resources.SID_Slot} {tceSlot.Position}";
            if (!tceSlot.IsPresent)
                line += @" - " + Resources.SID_not_configured;
            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText(line, TextFormat.Bold);
            paragraph.Format.Font.Size = 12;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.4);
            
            if (!tceSlot.IsPresent)
               return;
            paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.4);

            var thisSlotRelations = _readModel.GponPortRelations
                .Where(r => r.TceId == tce.Id && r.SlotPosition == tceSlot.Position).ToList();

            var table = section.AddTable();
            table.Borders.Width = 0.25;

            table.AddColumn(@"2.8cm").Format.Alignment = ParagraphAlignment.Left;
            table.AddColumn(@"4cm").Format.Alignment = ParagraphAlignment.Left;
            table.AddColumn(@"3.5cm").Format.Alignment = ParagraphAlignment.Left;
            table.AddColumn(@"1.2cm").Format.Alignment = ParagraphAlignment.Left;
            table.AddColumn(@"14.9cm").Format.Alignment = ParagraphAlignment.Left;

            var rowHeader = table.AddRow();
            rowHeader.VerticalAlignment = VerticalAlignment.Center;
            rowHeader.TopPadding = Unit.FromCentimeter(0.1);
            rowHeader.BottomPadding = Unit.FromCentimeter(0.1);
            rowHeader.Format.Font.Bold = true;
            rowHeader.Cells[0].AddParagraph(Resources.SID_interface__);
            rowHeader.Cells[1].AddParagraph(@"RTU");
            rowHeader.Cells[2].AddParagraph(Resources.SID_Bop);
            rowHeader.Cells[3].AddParagraph(Resources.SID_Port);
            rowHeader.Cells[4].AddParagraph(Resources.SID_Trace);

            var from = tce.TceTypeStruct.GponInterfaceNumerationFrom;
            for (int i = from; i < from + tceSlot.GponInterfaceCount; i++)
            {
                var row = table.AddRow();
                row.HeightRule = RowHeightRule.Exactly;
                row.Height = Unit.FromCentimeter(0.6);
                row.VerticalAlignment = VerticalAlignment.Center;

                row.Cells[0].AddParagraph($@"{i}");
                var relation = thisSlotRelations.FirstOrDefault(r => r.GponInterface == i);
                if (relation != null)
                {
                    var rtu = _readModel.Rtus.First(r => r.Id == relation.RtuId);
                    row.Cells[1].AddParagraph(rtu.Title);

                    row.Cells[2].AddParagraph((relation.OtauPortDto.IsPortOnMainCharon 
                        ? rtu.RtuMaker == RtuMaker.IIT
                            ? @"---" : Resources.SID_Main
                        : relation.OtauPortDto.NetAddress.Ip4Address));
                    row.Cells[3].AddParagraph(relation.OtauPortDto.OpticalPort.ToString());

                    var trace = _readModel.Traces.First(t => t.TraceId == relation.TraceId);
                    row.Cells[4].AddParagraph(trace.Title);
                }
            }
        }

        private void LetsGetStarted(Section section, TceS tce)
        {
            var headerFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\Reports\Header-landscape.png");
            section.AddImage(headerFileName);

            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText(Resources.SID_Setting_up_interaction_between_telecommunications_equipment_and_RTU, TextFormat.Bold);
            paragraph.Format.Font.Size = 20;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(1.4);

            var paragraph2 = section.AddParagraph();
            var line2 = $@"{Resources.SID_TCE_title} - {tce.Title}";
            paragraph2.AddFormattedText(line2, TextFormat.Bold);
            paragraph2.Format.Font.Size = 14;
            paragraph2.Format.SpaceBefore = Unit.FromCentimeter(0.4);

            var paragraph3 = section.AddParagraph();
            var line3 = $@"{Resources.SID_TCE_type} - {tce.TceTypeStruct.TypeTitle}";
            paragraph3.AddFormattedText(line3, TextFormat.Bold);
            paragraph3.Format.Font.Size = 14;
            paragraph3.Format.SpaceBefore = Unit.FromCentimeter(0.4);

            var paragraph4 = section.AddParagraph();
            var line4 = $@"IP - {tce.Ip}";
            paragraph4.AddFormattedText(line4, TextFormat.Bold);
            paragraph4.Format.Font.Size = 14;
            paragraph4.Format.SpaceBefore = Unit.FromCentimeter(0.4);

            var paragraph5 = section.AddParagraph();
            var line5 = $@"{Resources.SID_Process_snmp_traps} - {(tce.ProcessSnmpTraps ? Resources.SID_ON : Resources.SID_OFF)}";
            paragraph5.AddFormattedText(line5, TextFormat.Bold);
            paragraph5.Format.Font.Size = 14;
            paragraph5.Format.SpaceBefore = Unit.FromCentimeter(0.4);

            if (!string.IsNullOrEmpty(tce.Comment))
            {
                var paragraph6 = section.AddParagraph();
                paragraph6.AddFormattedText(tce.Comment, TextFormat.Bold);
                paragraph6.Format.Font.Size = 14;
                paragraph6.Format.SpaceBefore = Unit.FromCentimeter(0.4);
            }
        }

    }
}
