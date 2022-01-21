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
            if (!res.IsSuccessful)
                return res;
            var otauList = (List<VeexOtau>)res.ResponseObject;

            var rootRes = await LeaveNoMoreThanOneRootOtau(rtuDoubleAddress, otauList, dto);
            if (!rootRes.IsSuccessful)
                return rootRes;
            otauList = (List<VeexOtau>)rootRes.ResponseObject;


            var schemeRes = dto.IsFirstInitialization
                ? await SetChildOtausFirstInitialization(rtuDoubleAddress, otauList)
                : await SetChildOtausReInitialization(rtuDoubleAddress, otauList, dto);

            if (!schemeRes.IsSuccessful)
            {
                schemeRes.ErrorMessage = "Failed to adjust cascading scheme to client's one!"
                                         + Environment.NewLine + schemeRes.ResponseJson;
                return schemeRes;
            }

            return await GetOtauList(rtuDoubleAddress);
        }

        private async Task<HttpRequestResult> LeaveNoMoreThanOneRootOtau(DoubleAddress rtuDoubleAddress, List<VeexOtau> otauList, InitializeRtuDto dto)
        {
            if (otauList.Count(o => o.id.StartsWith("S1")) > 1)
            {
                var currentRootOtau = otauList.FirstOrDefault(o => o.id.StartsWith("S1") && o.connected);

                if (currentRootOtau == null)
                {
                    if (dto.IsFirstInitialization)
                        currentRootOtau = otauList.First(o => o.id.StartsWith("S1"));
                    else
                        currentRootOtau = otauList.FirstOrDefault(o => o.id == dto.MainVeexOtau.id)
                                          ?? otauList.First(o => o.id.StartsWith("S1"));
                }

                foreach (var otauS1 in otauList.Where(o => o.id.StartsWith("S1") && o.id != currentRootOtau.id))
                {
                    var deleteRes = await _d2RtuVeexLayer1.DeleteOtau(rtuDoubleAddress, otauS1.id);
                    if (!deleteRes.IsSuccessful)
                        return deleteRes;
                }
            }

            return await GetOtauList(rtuDoubleAddress);
        }

        private async Task<HttpRequestResult> SetChildOtausReInitialization(DoubleAddress rtuDoubleAddress, List<VeexOtau> otauList, InitializeRtuDto dto)
        {
            var mainOtau = otauList.First(o => o.id.StartsWith("S1"));
            foreach (var veexOtau in otauList.Where(veexOtau => veexOtau.id != mainOtau.id))
            {
                var oldOtau = dto.Children.Values.FirstOrDefault(o => o.OtauId == veexOtau.id);
                if (oldOtau == null)
                {
                    var deleteResult = await _d2RtuVeexLayer1.DeleteOtau(rtuDoubleAddress, veexOtau.id);
                    if (!deleteResult.IsSuccessful)
                        return deleteResult;
                }
                else
                {
                    var port = dto.Children.FirstOrDefault(p => p.Value.OtauId == veexOtau.id).Key;
                    if (port >= mainOtau.portCount)
                    {
                        var deleteResult = await _d2RtuVeexLayer1.DeleteOtau(rtuDoubleAddress, veexOtau.id);
                        if (!deleteResult.IsSuccessful)
                            return deleteResult;
                    }
                }
            }

            foreach (var otauDto in dto.Children.Where(k => k.Key < mainOtau.portCount).Select(p => p.Value))
            {
                if (otauList.All(o => o.id != otauDto.OtauId))
                {
                    var createRes = await _d2RtuVeexLayer1.CreateOtau(rtuDoubleAddress, new NewOtau());
                    if (!createRes.IsSuccessful)
                        return createRes;
                }
            }

            var res = await GetOtauList(rtuDoubleAddress);
            if (!res.IsSuccessful)
                return res;
            otauList = (List<VeexOtau>)res.ResponseObject;

            return await AdjustSchemeToClientsView(rtuDoubleAddress, mainOtau.id, dto.Children, otauList);
        }

        private async Task<HttpRequestResult> SetChildOtausFirstInitialization(DoubleAddress rtuDoubleAddress, List<VeexOtau> otauList)
        {
            foreach (var veexOtau in otauList.Where(veexOtau => !veexOtau.id.StartsWith("S1")))
            {
                var deleteResult = await _d2RtuVeexLayer1.DeleteOtau(rtuDoubleAddress, veexOtau.id);
                if (!deleteResult.IsSuccessful)
                    return deleteResult;
            }

            var mainOtau = otauList.FirstOrDefault(o => o.id.StartsWith("S1"));
            return mainOtau == null
                ? await ResetCascadingScheme(rtuDoubleAddress)
                : await SetRootOtauIntoScheme(rtuDoubleAddress, mainOtau.id);
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

        private async Task<HttpRequestResult> ResetCascadingScheme(DoubleAddress rtuDoubleAddress)
        {
            var scheme = new VeexOtauCascadingScheme()
            {
                rootConnections = new List<RootConnection>(),
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
    }
}
