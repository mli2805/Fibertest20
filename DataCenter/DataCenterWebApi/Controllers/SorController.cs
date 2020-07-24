using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.IO;
using Optixsoft.SorFormat.Protobuf;

namespace Iit.Fibertest.DataCenterWebApi
{
    [Route("[controller]")]
    [ApiController]
    public class SorController : ControllerBase
    {
        private readonly IMyLog _logFile;
        private readonly WebC2DWcfManager _webC2DWcfManager;
        private readonly CommonC2DWcfManager _commonC2DWcfManager;
        private readonly DoubleAddress _doubleAddressForWebWcfManager;
        private readonly DoubleAddress _doubleAddressForCommonWcfManager;
        private readonly string _localIpAddress;

        public SorController(IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _webC2DWcfManager = new WebC2DWcfManager(iniFile, logFile);
            _doubleAddressForWebWcfManager = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToWebClient);
            _commonC2DWcfManager = new CommonC2DWcfManager(iniFile, logFile);
            _doubleAddressForCommonWcfManager = iniFile.ReadDoubleAddress((int)TcpPorts.ServerListenToCommonClient);
            _localIpAddress = iniFile.Read(IniSection.ClientLocalAddress, 11080).Ip4Address;
        }

        private string GetRemoteAddress()
        {
            var ip1 = HttpContext.Connection.RemoteIpAddress.ToString();
            // browser started on the same pc as this service
            return ip1 == "::1" ? _localIpAddress : ip1;
        }

        [Authorize]
        [HttpGet("Get-sor-octetstream")]
        public async Task<FileResult> GetSorAsOctetStream(bool isSorFile, int sorFileId, string measGuid, bool isBaseIncluded, bool isVxSor)
        {
            Guid measId = Guid.Empty;
            if (!isSorFile)
                Guid.TryParse(measGuid, out measId);

            var sorBytes = isSorFile
                ? await _commonC2DWcfManager
                    .SetServerAddresses(_doubleAddressForCommonWcfManager, User.Identity.Name, GetRemoteAddress())
                    .GetSorBytes(sorFileId)
                : await _webC2DWcfManager
                    .SetServerAddresses(_doubleAddressForWebWcfManager, User.Identity.Name, GetRemoteAddress())
                    .GetClientMeasurementResult(User.Identity.Name, measId);

            if (sorBytes == null)
            {
                _logFile.AppendLine($"Failed to get sor file {sorFileId}");
                return null;
            }

            _logFile.AppendLine($"Got sor file: {sorBytes.Length} bytes");

            if (isVxSor)
                sorBytes = await VxSor(sorBytes, isBaseIncluded);
            else
            {
                _logFile.AppendLine($"Sor file for saving, isBaseIncluded {isBaseIncluded}");
                if (!isBaseIncluded)
                {
                    _logFile.AppendLine($"Get rid of base");
                    sorBytes = SorData.GetRidOfBase(sorBytes);
                }
            }
            _logFile.AppendLine($"After transformations: {sorBytes.Length} bytes");

            var stream = new MemoryStream(sorBytes);
            return File(stream, "application/octet-stream", "unused_so_far");
        }

        private async Task<byte[]> VxSor(byte[] sorBytes, bool isBase)
        {
            OtdrDataKnownBlocks otdrDataKnownBlocks;
            await using (var stream = new MemoryStream(sorBytes))
                otdrDataKnownBlocks = new OtdrDataKnownBlocks(new OtdrReader(stream).Data);

            var to = isBase ? await GetOnlyBaseFromSor(otdrDataKnownBlocks) : otdrDataKnownBlocks;
            var protobuf = to.ToSorDataBuf();
            return protobuf.ToBytes();
        }

        private async Task<OtdrDataKnownBlocks> GetOnlyBaseFromSor(OtdrDataKnownBlocks otdrData)
        {
            var embeddedData = otdrData.EmbeddedData.EmbeddedDataBlocks.FirstOrDefault(b => b.Description == @"SOR");
            if (embeddedData == null) return null;

            OtdrDataKnownBlocks baseOtdrData;
            await using (var stream = new MemoryStream(embeddedData.Data))
                baseOtdrData = new OtdrDataKnownBlocks(new OtdrReader(stream).Data);

            return baseOtdrData;
        }
    }
}