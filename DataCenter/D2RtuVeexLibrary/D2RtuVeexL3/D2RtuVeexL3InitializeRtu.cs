using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer3
    {
        public async Task<RtuInitializedDto> InitializeRtu(InitializeRtuDto dto)
        {
            try
            {
                var rtuAddresses = (DoubleAddress)dto.RtuAddresses.Clone();
                var rtuInitializedDto = await _d2RtuVeexLayer2.GetSettings(rtuAddresses, dto);
                if (!rtuInitializedDto.IsInitialized)
                    return rtuInitializedDto;

                if (!await AdjustCascadingScheme(rtuAddresses, dto, rtuInitializedDto))
                    return rtuInitializedDto;

                var initRes = await _d2RtuVeexLayer2.InitializeMonitoringProperties(rtuAddresses);
                if (initRes != null)
                {
                    rtuInitializedDto.ErrorMessage = initRes.ErrorMessage;
                    rtuInitializedDto.ReturnCode = initRes.ReturnCode;
                    return rtuInitializedDto;
                }

                var saveRes = await _d2RtuVeexLayer2.SetServerNotificationSettings(rtuAddresses, dto);
                if (saveRes.HttpStatusCode != HttpStatusCode.NoContent)
                    return new RtuInitializedDto()
                    {
                        ReturnCode = ReturnCode.RtuInitializationError,
                        ErrorMessage = saveRes.ErrorMessage,
                    };

                rtuInitializedDto.ReturnCode = ReturnCode.RtuInitializedSuccessfully;
                return rtuInitializedDto;
            }
            catch (Exception e)
            {
                return new RtuInitializedDto()
                {
                    ReturnCode = ReturnCode.RtuInitializationError,
                    ErrorMessage = e.Message,
                };
            }
        }

        private async Task<bool> AdjustCascadingScheme(DoubleAddress rtuDoubleAddress,
            InitializeRtuDto dto, RtuInitializedDto rtuInitializedDto)
        {

            // adjust cascading scheme to Client's one
            var adjustRes = await _d2RtuVeexLayer2.AdjustCascadingScheme(rtuDoubleAddress,
                CreateScheme(rtuInitializedDto.OtauId, dto.Children));
            if (adjustRes.HttpStatusCode != HttpStatusCode.NoContent)
            {
                rtuInitializedDto.ReturnCode = ReturnCode.RtuInitializationError;
                rtuInitializedDto.ErrorMessage = "Failed to adjust cascading scheme to client's one!"
                                                 + Environment.NewLine + adjustRes.ErrorMessage;
                return false;
            }

            return true;
        }

        private static VeexOtauCascadingScheme CreateScheme(string mainOtauId, Dictionary<int, OtauDto> children)
        {
            var scheme = new VeexOtauCascadingScheme()
            {
                rootConnections = new List<RootConnection>()
                {
                    new RootConnection(){ inputOtauId = mainOtauId, inputOtauPort = 0 }
                },
                connections = new List<Connection>()
            };
            foreach (var child in children)
            {
                scheme.connections.Add(new Connection()
                {
                    inputOtauId = mainOtauId,
                    inputOtauPort = 0,
                    outputOtauId = child.Value.OtauId,
                    outputOtauPort = child.Key - 1,
                });
            }
            return scheme;
        }
    }
}
