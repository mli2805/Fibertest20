using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using SnmpSharpNet;

namespace Iit.Fibertest.DataCenterCore
{
    public class OltTrapParser
    {
        private readonly IMyLog _logFile;

        public OltTrapParser(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public OltTrapParserResult Parse(SnmpV2Packet pkt, Olt olt)
        {
            switch (olt.OltModel)
            {
                case OltModel.Huawei_MA5608T: return ParseHuawei(pkt);
                case OltModel.ZTE_ZXA10C320: return ParseZte(pkt);
                default:
                    _logFile.AppendLine($"Parser for OLT model {olt.OltModel} is not implemented");
                    return null;
            }
        }

        private OltTrapParserResult ParseHuawei(SnmpV2Packet pkt)
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

            return new OltTrapParserResult()
            {
                GponInterface = gpon,
                State = pkt.Pdu[11].ToString() == "1" ? FiberState.Ok : FiberState.FiberBreak,
            };
        }

        private OltTrapParserResult ParseZte(SnmpV2Packet pkt)
        {
            var community = pkt.Community.ToString();
            var ss = community.Split('@');

            return new OltTrapParserResult
            {
                State = ss[2] == "eventLevel=cleared" ? FiberState.Ok : FiberState.FiberBreak
            };
        }
    }
}