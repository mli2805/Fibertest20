using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using SnmpSharpNet;
using System.Linq;
using System.Net;

namespace Iit.Fibertest.DataCenterCore
{
    public class TrapParser
    {
        private readonly IMyLog _logFile;
        private readonly Model _writeModel;

        public TrapParser(IMyLog logFile, Model writeModel)
        {
            _logFile = logFile;
            _writeModel = writeModel;
        }

        public TrapParserResult ParseTrap(SnmpV2Packet pkt, EndPoint endPoint)
        {
            var ss = endPoint.ToString().Split(':');
            var tce = _writeModel.TcesNew.FirstOrDefault(o => o.Ip == ss[0]);
            if (tce == null)
            {
                _logFile.AppendLine($"Unknown trap source address: {ss[0]}", 3);
                return null;
            }

            if (!tce.ProcessSnmpTraps)
            {
                _logFile.AppendLine($"Trap processing of {tce.Title} {tce.Ip} is turned off");
                return null;
            }

            var res = ParsePaket(pkt, tce);


            if (res != null)
                res.TceId = tce.Id;

            LogParsedTrap(res, tce);
            return res;
        }

        private void LogParsedTrap(TrapParserResult trapParserResult, TceS tce)
        {
            _logFile.EmptyLine();
            if (trapParserResult != null)
            {
                var tcem = $"TCE: {tce.Title} {tce.Ip}";
                var slot = $"Slot: {trapParserResult.Slot}";
                var gpon = $"GponInterface: {trapParserResult.GponInterface}";
                var state = $"Trace state: {trapParserResult.State}";
                _logFile.AppendLine($"{tcem} {slot} {gpon} {state}");
            }
            else
            {
                _logFile.AppendLine($"Not a line event trap from {tce.Title} {tce.Ip}", 0, 3);
            }
        }

        private TrapParserResult ParsePaket(SnmpV2Packet pkt, TceS tce)
        {
            switch (tce.TceTypeStruct.Code)
            {
                case @"Huawei_MA5600T_R016":
                    return pkt.ParseMa5600T_R016();
                case @"Huawei_MA5600T_R018":
                    return pkt.ParseMa5600T_R018();
                case @"Huawei_MA5608T_R013":
                    return pkt.ParseMa5608T_R013();

                case @"ZTE_C300_v1": return pkt.ParseC300();
                case @"ZTE_C300M_v4": return pkt.ParseC300M40();
                case @"ZTE_C300M_v43": return pkt.ParseC300M43();
                case @"ZTE_C320_v1": return pkt.ParseC320();
                default:
                    _logFile.AppendLine($"Parser for OLT model {tce.TceTypeStruct.Code} is not implemented");
                    return null;
            }
        }
    }
}