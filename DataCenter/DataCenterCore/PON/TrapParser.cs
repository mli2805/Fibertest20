using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using SnmpSharpNet;

namespace Iit.Fibertest.DataCenterCore
{
    public class TrapParser
    {
        private readonly IMyLog _logFile;

        public TrapParser(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public TrapParserResult Parse(SnmpV2Packet pkt, TceS tce)
        {
            switch (tce.TceTypeStruct.Code)
            {
                case @"Huawei_MA5600T_v1": 
                case @"Huawei_MA5600T_v2": 
                case @"Huawei_MA5600T_v3": 
                    return pkt.ParseMa5600T(tce);
                case @"Huawei_MA5608T_v1": return ParseHuawei(pkt);
                case @"ZTE_C300_v1": return pkt.ParseC300(tce);
                case @"ZTE_C300M_v1": return pkt.ParseC300M(tce);
                case @"ZTE_C320_v1": return ParseZte(pkt);
                default:
                    _logFile.AppendLine($"Parser for OLT model {tce.TceTypeStruct.Code} is not implemented");
                    return null;
            }
        }

        private TrapParserResult ParseHuawei(SnmpV2Packet pkt)
        {
            if (pkt.Pdu.VbCount < 12)
            {
                _logFile.AppendLine("Not a fiber state trap");
                return null; // not a fiber state trap
            }

            var gponVb = pkt.Pdu[7].Value.ToString();
            if (!int.TryParse(gponVb, out int gpon))
            {
                _logFile.AppendLine($"Not a fiber state trap. Gpon number is {gponVb}");
                return null;
            }

            return new TrapParserResult()
            {
                GponInterface = gpon,
                State = pkt.Pdu[11].ToString() == "1" ? FiberState.Ok : FiberState.FiberBreak,
            };
        }

        private TrapParserResult ParseZte(SnmpV2Packet pkt)
        {
            var community = pkt.Community.ToString();
            var ss = community.Split('@');

            return new TrapParserResult
            {
                State = ss[2] == "eventLevel=cleared" ? FiberState.Ok : FiberState.FiberBreak
            };
        }
    }
}