using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using SnmpSharpNet;

namespace Iit.Fibertest.DataCenterCore
{
    public static class HuaweiTrapParser
    {
        public static TrapParserResult ParseMa5600T_R018(this SnmpV2Packet pkt, TceS tce)
        {
            if (pkt.Pdu.VbCount < 11) return null;
            var trapCode = pkt.Pdu[0].Value + pkt.Pdu[1].Value.ToString() + pkt.Pdu[2].Value;
            if (trapCode != "043") return null;

            var result = new TrapParserResult()
            {
                TceId = tce.Id,
                Slot = int.Parse(pkt.Pdu[6].Value.ToString()),
                GponInterface = int.Parse(pkt.Pdu[7].Value.ToString()),
                State = pkt.Pdu[11].Value.ToString() == "2" ? FiberState.FiberBreak : FiberState.Ok,
            };
            return result;
        }

        public static TrapParserResult ParseMa5600T_R016(this SnmpV2Packet pkt, TceS tce)
        {
            if (pkt.Pdu.VbCount < 10) return null;
            var trapCode = pkt.Pdu[0].Value + pkt.Pdu[1].Value.ToString() + pkt.Pdu[2].Value;
            if (trapCode != "043") return null;

            // 043 + 355 - is a problem with ONT modem
            if (pkt.Pdu[4].Value.ToString() != "416") return null;

            var result = new TrapParserResult()
            {
                TceId = tce.Id,
                Slot = int.Parse(pkt.Pdu[6].Value.ToString()),
                GponInterface = int.Parse(pkt.Pdu[7].Value.ToString()),
                State = pkt.Pdu[11].Value.ToString() == "2" ? FiberState.FiberBreak : FiberState.Ok,
            };
            return result;
        }

        public static TrapParserResult ParseMa5608T_R013(this SnmpV2Packet pkt, TceS tce)
        {
            if (pkt.Pdu.VbCount < 12)
                return null; // not a fiber state trap

            // 043 + 355 - is a problem with ONT modem
            if (pkt.Pdu[4].Value.ToString() != "416") return null;

            return new TrapParserResult()
            {
                TceId = tce.Id,
                Slot = int.Parse(pkt.Pdu[6].Value.ToString()),
                GponInterface = int.Parse(pkt.Pdu[7].Value.ToString()),
                State = pkt.Pdu[11].Value.ToString() == "2" ? FiberState.FiberBreak : FiberState.Ok,
            };
        }
    }
}