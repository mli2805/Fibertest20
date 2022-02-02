using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public static class InitializedDtoFactory
    {
        public static RtuInitializedDto FillInRtuSettings(this RtuInitializedDto result, InitializeRtuDto dto)
        {
            result.RtuId = dto.RtuId;
            result.Maker = RtuMaker.VeEX;
            result.RtuAddresses = dto.RtuAddresses;
            result.OtdrAddress = (NetAddress)dto.RtuAddresses.Main.Clone();
            result.ReturnCode = ReturnCode.RtuInitializedSuccessfully;
            result.IsInitialized = true;

            return result;
        }

        public static RtuInitializedDto FillInPlatform(this RtuInitializedDto result, VeexPlatformInfo info)
        {
            result.Mfid = info.platform.name;
            result.Mfsn = info.platform.serialNumber;
            result.Serial = info.platform.serialNumber;
            result.Version = info.components.api;
            result.Version2 = info.components.otdrEngine.iit_otdr;

            return result;
        }

        public static RtuInitializedDto FillInOtdr(this RtuInitializedDto result, VeexOtdr otdr)
        {
            result.OtdrId = otdr.id;
            result.Omid = otdr.mainframeId;
            result.Omsn = otdr.opticalModuleSerialNumber;
            result.AcceptableMeasParams = new TreeOfAcceptableMeasParams();
            foreach (var laserUnitPair in otdr.supportedMeasurementParameters.laserUnits)
            {
                var branch = new BranchOfAcceptableMeasParams
                {
                    BackscatteredCoefficient = -81,
                    RefractiveIndex = 1.4682
                };
                foreach (var distancePair in laserUnitPair.Value.distanceRanges)
                {
                    var leaf = new LeafOfAcceptableMeasParams
                    {
                        Resolutions = distancePair.Value.resolutions,
                        PulseDurations = distancePair.Value.pulseDurations,
                        MeasCountsToAverage = distancePair.Value.fastAveragingTimes,
                        PeriodsToAverage = distancePair.Value.averagingTimes
                    };
                    branch.Distances.Add(distancePair.Key.ToString(CultureInfo.InvariantCulture), leaf);
                }

                result.AcceptableMeasParams.Units.Add(laserUnitPair.Key, branch);
            }

            return result;
        }

        public static RtuInitializedDto FillInOtau(this RtuInitializedDto result, List<VeexOtau> otauList, InitializeRtuDto dto)
        {
            if (otauList.Count == 0 || (otauList.Count == 1 && !otauList[0].connected && dto.IsFirstInitialization))
            {
                result.OwnPortCount = 1;
                result.FullPortCount = 1;
                result.Children = new Dictionary<int, OtauDto>();

                return result;
            }

            var mainOtau = otauList.First(o => o.id.StartsWith("S1"));

            result.MainVeexOtau = mainOtau;

            result.OwnPortCount = mainOtau.portCount;
            result.FullPortCount = mainOtau.portCount;
            result.Children = new Dictionary<int, OtauDto>();

            foreach (var pair in dto.Children)
            {
                var otau = otauList.FirstOrDefault(o => o.id == "S2_" + pair.Value.OtauId);
                if (otau != null)
                {
                    result.Children.Add(pair.Key, pair.Value);
                    result.FullPortCount += pair.Value.OwnPortCount - 1;
                }
            }

            return result;
        }
    }
}
