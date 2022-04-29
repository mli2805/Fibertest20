using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using SnmpSharpNet;

namespace Iit.Fibertest.DataCenterCore
{
    public static class ZteTrapParser
    {
        public static TrapParserResult ParseC320(this SnmpV2Packet pkt, TceS tce)
        {
            var community = pkt.Community.ToString();
            var ss = community.Split('@');

            if (ss.Length < 3 || (ss[2] != "eventLevel=critical" && ss[2] != "eventLevel=cleared")) return null;

            var pdu = pkt.Pdu[0];
            if (pdu.Oid.ToString() != "1.3.6.1.2.1.2.2.1.1") return null;

            var codeStr = pdu.Value.ToString();
            if (!int.TryParse(codeStr, out int code)) return null;

            return CreateResult(tce, ss[2], code);
        }

        public static TrapParserResult ParseC300(this SnmpV2Packet pkt, TceS tce)
        {
            var community = pkt.Community.ToString();
            var ss = community.Split('@');

            if (ss.Length < 3 || (ss[2] != "eventLevel=critical" && ss[2] != "eventLevel=cleared")) return null;

            var pdu = pkt.Pdu[0];
            if (pdu.Oid.ToString() != "1.3.6.1.2.1.2.2.1.1") return null;

            var codeStr = pdu.Value.ToString();
            if (!int.TryParse(codeStr, out int code)) return null;

            return CreateResult(tce, ss[2], code);
        }

        public static TrapParserResult ParseC300M(this SnmpV2Packet pkt, TceS tce)
        {
            var community = pkt.Community.ToString();
            var ss = community.Split('@');

            if (ss.Length < 3 || (ss[2] != "eventLevel=critical" && ss[2] != "eventLevel=cleared")) return null;

            var oid = pkt.Pdu[0].Oid.ToString();
            var point = oid.LastIndexOf('.');
            var oidMinus = oid.Substring(0, point);
            if (oidMinus != "1.3.6.1.4.1.3902.1082.500.10.2.2.3.1.1") return null;

            var codeStr = oid.Substring(point + 1);
            if (!int.TryParse(codeStr, out int code)) return null;

            return CreateResult(tce, ss[2], code);
        }

        /// <summary>
        /// zte code 0x03020100
        /// 
        /// zte c300 & c320 => on place 02 - slot; on place 01 - interface
        /// 
        /// zte c300M => on place 01 - slot; on place 00 - interface
        /// </summary>
        /// <param name="code">ZTE code 0x03020100</param>
        /// <param name="place">4 places in ZTE code (3 -> 0)</param>
        /// <returns></returns>
        public static int GetNumber(this int code, int place)
        {
            var shifted = code >> (place * 8);
            return shifted & 0x00000011;
        }

        private static TrapParserResult CreateResult(TceS tce, string eventLevel, int code)
        {
            var result = new TrapParserResult
            {
                TceId = tce.Id,
                Slot = tce.TceTypeStruct.Code == "ZTE_C300M_v4" ? code.GetNumber(1) : code.GetNumber(2),
                GponInterface = tce.TceTypeStruct.Code == "ZTE_C300M_v4" ? code.GetNumber(0) : code.GetNumber(1),
                State = eventLevel == "eventLevel=critical" ? FiberState.FiberBreak : FiberState.Ok,
            };
            return result;
        }
    }
}