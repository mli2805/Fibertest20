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
                case @"Huawei_MA5600T_R016":
                    return pkt.ParseMa5600T_R016(tce);
                case @"Huawei_MA5600T_R018": 
                    return pkt.ParseMa5600T_R018(tce);
                case @"Huawei_MA5608T_R013": 
                    return pkt.ParseMa5608T_R013(tce);

                case @"ZTE_C300_v1": return pkt.ParseC300(tce);
                case @"ZTE_C300M_v4": return pkt.ParseC300M(tce);
                case @"ZTE_C320": return pkt.ParseC320(tce);
                default:
                    _logFile.AppendLine($"Parser for OLT model {tce.TceTypeStruct.Code} is not implemented");
                    return null;
            }
        }
    }
}