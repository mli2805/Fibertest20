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

        public TrapParserResult Parse(SnmpV2Packet pkt, Tce tce)
        {
            switch (tce.TceType)
            {
                case TceType.Huawei_MA5600T: return pkt.ParseMa5600T(tce);
                case TceType.Huawei_MA5608T: return ParseHuawei(pkt);
                case TceType.ZTE_C300: return pkt.ParseC300(tce);
                case TceType.ZTE_C300M: return pkt.ParseC300M(tce);
                case TceType.ZTE_ZXA10C320: return ParseZte(pkt);
                default:
                    _logFile.AppendLine($"Parser for OLT model {tce.TceType} is not implemented");
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