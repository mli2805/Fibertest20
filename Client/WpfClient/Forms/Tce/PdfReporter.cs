using System;
using Iit.Fibertest.StringResources;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace Iit.Fibertest.Client
{
    public static class PdfReporter
    {
        public static void SetLandscapeFooter(this Section section, string title)
        {
            Table table = new Table();
            table.Borders.Width = 0;

            table.AddColumn(@"4.2cm").Format.Alignment = ParagraphAlignment.Left;
            table.AddColumn(@"15.3cm").Format.Alignment = ParagraphAlignment.Left;
            table.AddColumn(@"3.5cm").Format.Alignment = ParagraphAlignment.Left;
            table.AddColumn(@"3.2cm").Format.Alignment = ParagraphAlignment.Right;

            var row = table.AddRow();
            row.Cells[0].AddParagraph(@"Fibertest 2.0 ©");
            row.Cells[1].AddParagraph(title);
            row.Cells[2].AddParagraph($@"{DateTime.Now:g}");
            
            Paragraph page = new Paragraph();
            page.AddFormattedText(Resources.SID_Page_);
            page.AddPageField();
            page.AddFormattedText(@" / ");
            page.AddNumPagesField();
            row.Cells[3].Add(page);

            table.Format.Font.Size = 10;
            table.Format.Alignment = ParagraphAlignment.Left;

            section.Footers.Primary.Add(table);
            section.Footers.EvenPage.Add(table.Clone());
        }
    }
}