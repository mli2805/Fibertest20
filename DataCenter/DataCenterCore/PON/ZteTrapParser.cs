using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using SnmpSharpNet;

namespace Iit.Fibertest.DataCenterCore
{
    public static class ZteTrapParser
    {
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

        public static int GetSlot(this int code)
        {
            var shift = code >> 8;
            var result = shift & 0x00000011;
            return result;
        }

        public static int GetGponInterface(this int code)
        {
            var result = code & 0x00000011;
            return result;
        }

        private static TrapParserResult CreateResult(TceS tce, string eventLevel, int code)
        {
            var result = new TrapParserResult
            {
                TceId = tce.Id,
                Slot = code.GetSlot(),
                GponInterface = code.GetGponInterface(),
                State = eventLevel == "eventLevel=critical" ? FiberState.Critical : FiberState.Ok,
            };
            return result;
        }
    }
}