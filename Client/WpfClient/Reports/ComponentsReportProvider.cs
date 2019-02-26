using System;
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
    public class ComponentsReportProvider
    {
        private readonly Model _readModel;
        private readonly TreeOfRtuModel _tree;
        private readonly CurrentDatacenterParameters _server;

        public ComponentsReportProvider(Model readModel, TreeOfRtuModel tree, CurrentDatacenterParameters server)
        {
            _readModel = readModel;
            _tree = tree;
            _server = server;
        }

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

            foreach (var rtu in _readModel.Rtus)
            {
                ProcessRtu(section, rtu);
            }

            PdfDocumentRenderer pdfDocumentRenderer =
                new PdfDocumentRenderer(true) { Document = doc };
            pdfDocumentRenderer.RenderDocument();

            return pdfDocumentRenderer.PdfDocument;
        }

        private void LetsGetStarted(Section section)
        {
            var headerFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Resources\Reports\Header.png");
            var image = section.AddImage(headerFileName);
//            image.Width = Unit.FromCentimeter(17);
            image.LockAspectRatio = true;

            var paragraph = section.AddParagraph();
            paragraph.AddFormattedText(Resources.SID_Monitoring_system_components, TextFormat.Bold);
            paragraph.Format.Font.Size = 20;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(1.4);
            var paragraph2 = section.AddParagraph();
            var software = string.Format(Resources.SID_software____0_, _server.DatacenterVersion);
            var server = string.Format(Resources.SID_Server_____0_____1_____2_, _server.ServerTitle, _server.ServerIp, software);
            paragraph2.AddFormattedText(server, TextFormat.Bold);
            paragraph2.Format.Font.Size = 14;
            paragraph2.Format.SpaceBefore = Unit.FromCentimeter(0.4);
        }

        private void SetFooter(Section section)
        {
            Paragraph footer = new Paragraph();
            var reportNameInFooter = string.Format(Resources.SID_Components_of_monitoring_system__0_, _server.ServerTitle);
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

        private void ProcessRtu(Section section, Rtu rtu)
        {
            var paragraph = section.AddParagraph();
            var mode = rtu.MonitoringState == MonitoringState.On ? "AUTO" : "MANUAL";
            var availability = rtu.IsAvailable ? "AVAILABLE" : "NOT AVAILABLE";
            var serial = string.Format(Resources.SID_s_n__0_, rtu.Serial);
            var portCount = string.Format(Resources.SID_ports____0_, rtu.PortCount);
            var software = string.Format(Resources.SID_software____0_, rtu.Version);
            paragraph.AddFormattedText($@"{rtu.Title} ; {serial} ; {portCount} ; {software} ; {mode} ; {availability}");
            var mainChannel = string.Format(Resources.SID____Main_channel____0_, rtu.MainChannel.ToStringA());
            var reserveChannel = rtu.IsReserveChannelSet
                ? string.Format(Resources.SID____Reserve_channel____0_, rtu.ReserveChannel.ToStringA()) : "";
            paragraph.AddFormattedText($@"{mainChannel}{reserveChannel}");

            paragraph.Format.Font.Size = 12;
            paragraph.Format.Font.Bold = true;
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.5);

            var rtuLeaf = (RtuLeaf)_tree.GetById(rtu.Id);
            foreach (var child in rtuLeaf.ChildrenImpresario.Children)
            {
                if (child is TraceLeaf traceLeaf)
                    ProcessTrace(section, traceLeaf);
                else if (child is OtauLeaf otauLeaf)
                    ProcessBop(section, otauLeaf);
            }
        }

        private void ProcessBop(Section section, OtauLeaf otauLeaf)
        {
            var bop = _readModel.Otaus.First(o => o.Id == otauLeaf.Id);
            var paragraph = section.AddParagraph();
            paragraph.Format.Font.Size = 12;
            paragraph.Format.Font.Bold = true;
            var port = Resources.SID_Port;
            var serial = string.Format(Resources.SID_s_n__0_, bop.Serial);
            var state = bop.IsOk ? "AVAILABLE" : "NOT AVAILABLE";
            paragraph.AddFormattedText($@"{port} {bop.MasterPort} ; {bop.NetAddress.ToStringA()} ; {serial} ; {state}");
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.2);
            paragraph.Format.FirstLineIndent = Unit.FromCentimeter(1);

            foreach (var child in otauLeaf.ChildrenImpresario.Children)
            {
                if (child is TraceLeaf traceLeaf)
                    ProcessTrace(section, traceLeaf, bop.MasterPort);
            }
        }

        private void ProcessTrace(Section section, TraceLeaf traceLeaf, int otauPort = 0)
        {
            var trace = _readModel.Traces.First(t => t.TraceId == traceLeaf.Id);
            var paragraph = section.AddParagraph();
            paragraph.Format.Font.Size = 12;
            var otauPortNumber = otauPort != 0 ? $@"{otauPort}-" : "";
            var portNumber = trace.Port != -1 ? string.Format(Resources.SID_port__0__1____, otauPortNumber, trace.Port) : "";

            paragraph.AddFormattedText($@"{portNumber}{trace.Title} ; {trace.State.ToLocalizedString()}");
            paragraph.Format.SpaceBefore = Unit.FromCentimeter(0.2);
            paragraph.Format.FirstLineIndent = Unit.FromCentimeter(otauPort == 0 ? 1 : 1.5);
        }
    }
}
