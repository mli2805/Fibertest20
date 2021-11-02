using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        /// <summary>
        /// Not only gets otau list, but makes changes if it does not match with what client sees
        /// </summary>
        /// <param name="rtuDoubleAddress"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<HttpRequestResult> InitializeOtaus(DoubleAddress rtuDoubleAddress, InitializeRtuDto dto)
        {
            var res = await GetOtauList(rtuDoubleAddress);
            if (!res.IsSuccessful) return res;
            var otauList = (List<VeexOtau>)res.ResponseObject;

            var resScheme = await _d2RtuVeexLayer1.GetOtauCascadingScheme(rtuDoubleAddress);
            if (!resScheme.IsSuccessful) return resScheme;

            if (otauList.Count > 0)
            {
                if (dto.IsFirstInitialization)
                {
                    // first initialization
                    var resetRes = await LeaveOnlyRootOtau(rtuDoubleAddress, otauList);
                    if (!resetRes.IsSuccessful)
                        return resetRes;

                    dto.OtauId = resetRes.ResponseJson;

                    var initSchemeRes = await SetRootOtauIntoScheme(rtuDoubleAddress, dto.OtauId);
                    if (!initSchemeRes.IsSuccessful)
                    {
                        initSchemeRes.ErrorMessage = "Failed to set main OTAU as a root in cascading scheme!"
                                                     + Environment.NewLine + initSchemeRes.ErrorMessage;
                        return initSchemeRes;
                    }
                }

                var adjustRes = await AdjustSchemeToClientsView(rtuDoubleAddress, dto.OtauId, dto.Children, otauList);
                if (!adjustRes.IsSuccessful)
                {
                    adjustRes.ErrorMessage = "Failed to adjust cascading scheme to client's one!"
                                             + Environment.NewLine + adjustRes.ResponseJson;
                    return adjustRes;
                }
            }

            res.ResponseObject = new VeexOtauInfo()
            {
                OtauList = otauList,
                OtauScheme = (VeexOtauCascadingScheme)resScheme.ResponseObject,
            };
            return res;
        }

        private async Task<HttpRequestResult> LeaveOnlyRootOtau(DoubleAddress rtuDoubleAddress, List<VeexOtau> otauList)
        {
            string mainOtauId = "";
            // temporary, certainly need to be more reliable attribute
            foreach (var veexOtau in otauList)
            {
                if (veexOtau.protocol.StartsWith("tcpip"))
                {
                    var deleteResult = await _d2RtuVeexLayer1.DeleteOtau(rtuDoubleAddress, veexOtau.id);
                    if (!deleteResult.IsSuccessful)
                        return deleteResult;
                }

                if (veexOtau.protocol.StartsWith("db25"))
                {
                    mainOtauId = veexOtau.id;
                }
            }

            var resetResult = await ResetCascadingScheme(rtuDoubleAddress);
            resetResult.ResponseJson = mainOtauId;
            return resetResult;
        }

        private async Task<HttpRequestResult> GetOtauList(DoubleAddress rtuDoubleAddress)
        {
            var res = await _d2RtuVeexLayer1.GetOtaus(rtuDoubleAddress);
            if (!res.IsSuccessful) return res;

            var otaus = (LinkList)res.ResponseObject;
            var otauList = new List<VeexOtau>();
            foreach (var link in otaus.items)
            {
                var resOtau = await _d2RtuVeexLayer1.GetOtau(rtuDoubleAddress, link.self);
                if (!resOtau.IsSuccessful) return resOtau;
                otauList.Add((VeexOtau)resOtau.ResponseObject);
            }

            res.ResponseObject = otauList;
            return res;
        }

        private async Task<HttpRequestResult> ResetCascadingScheme(DoubleAddress rtuDoubleAddress)
        {
            var scheme = new VeexOtauCascadingScheme()
            {
                rootConnections = new List<RootConnection>(),
                connections = new List<Connection>(),
            };
            return await _d2RtuVeexLayer1.ChangeOtauCascadingScheme(rtuDoubleAddress, scheme);
        }

        private async Task<HttpRequestResult> SetRootOtauIntoScheme(DoubleAddress rtuDoubleAddress, string mainOtauId)
        {
            var scheme = new VeexOtauCascadingScheme()
            {
                rootConnections = new List<RootConnection>()
                {
                    new RootConnection()
                    {
                        inputOtauId = mainOtauId,
                        inputOtauPort = 0,
                    }
                },
                connections = new List<Connection>(),
            };
            return await _d2RtuVeexLayer1.ChangeOtauCascadingScheme(rtuDoubleAddress, scheme);
        }

        private async Task<HttpRequestResult> AdjustSchemeToClientsView(DoubleAddress rtuDoubleAddress, string mainOtauId,
            Dictionary<int, OtauDto> children, List<VeexOtau> otauList)
        {
            var scheme = new VeexOtauCascadingScheme()
            {
                rootConnections = new List<RootConnection>()
                {
                    new RootConnection()
                    {
                        inputOtauId = mainOtauId,
                        inputOtauPort = 0,
                    }
                },
                connections = new List<Connection>(),
            };

            foreach (var pair in children)
            {
                var veexOtau = otauList.FirstOrDefault(o => o.id == "S2_" + pair.Value.OtauId);
                if (veexOtau == null)
                {
                    var createRes = await _d2RtuVeexLayer1.CreateOtau(rtuDoubleAddress, new NewOtau()
                    {
                        id = "S2_" + pair.Value.OtauId,
                        connectionParameters = new VeexOtauAddress()
                        {
                            address = pair.Value.NetAddress.Ip4Address,
                            port = pair.Value.NetAddress.Port,
                        }
                    });
                    if (!createRes.IsSuccessful)
                        return createRes;
                    var getRes = await _d2RtuVeexLayer1.GetOtau(rtuDoubleAddress, createRes.ResponseJson);
                    if (!getRes.IsSuccessful)
                        return getRes;
                    veexOtau = (VeexOtau)getRes.ResponseObject;
                    otauList.Add(veexOtau);
                }

                scheme.connections.Add(new Connection()
                {
                    outputOtauId = mainOtauId,
                    outputOtauPort = pair.Key - 1,
                    inputOtauId = "S2_" + pair.Value.OtauId,
                    inputOtauPort = 0,
                });
            }
            return await _d2RtuVeexLayer1.ChangeOtauCascadingScheme(rtuDoubleAddress, scheme);
        }
    }
}
