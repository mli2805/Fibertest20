using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using SnmpSharpNet;

namespace Iit.Fibertest.DataCenterCore
{
    public static class TrapParser
    {
        public static TrapParserResult Parse(this SnmpV2Packet pkt, TceS tce, IMyLog logFile)
        {
            var trapParserResult = ParsePaket(pkt, tce, logFile);
            if (trapParserResult != null)
            {
                trapParserResult.TceId = tce.Id;

                // extra logging - remove after experiments
                {
                    logFile.EmptyLine();
                    logFile.AppendLine("Trap parsed successfully!");
                    logFile.AppendLine($"TCE: {tce.Title}");
                    logFile.AppendLine($"Slot: {trapParserResult.Slot}");
                    logFile.AppendLine($"GponInterface: {trapParserResult.GponInterface}");
                }

            }
            return trapParserResult;
        }

        private static TrapParserResult ParsePaket(SnmpV2Packet pkt, TceS tce, IMyLog logFile)
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
                case @"ZTE_C300M_v4": return pkt.ParseC300M();
                case @"ZTE_C320": return pkt.ParseC320();
                default:
                    logFile.AppendLine($"Parser for OLT model {tce.TceTypeStruct.Code} is not implemented");
                    return null;
            }
        }
    }
}