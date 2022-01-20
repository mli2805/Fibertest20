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

        public static RtuInitializedDto FillInOtau(this RtuInitializedDto result, VeexOtauInfo otauInfo, InitializeRtuDto dto)
        {
            if (otauInfo.OtauList.Count == 0)
            {
                result.OwnPortCount = 1;
                result.FullPortCount = 1;
                result.Children = new Dictionary<int, OtauDto>();

                return result;
            }

            var mainOtauId = otauInfo.OtauScheme.rootConnections[0].inputOtauId;
            var mainOtau = otauInfo.OtauList.First(o => o.id == mainOtauId);

            result.MainVeexOtau = mainOtau;

            result.OwnPortCount = mainOtau.portCount;
            result.FullPortCount = mainOtau.portCount;
            result.Children = new Dictionary<int, OtauDto>();

            foreach (var childConnection in otauInfo.OtauScheme.connections)
            {
                var pair = dto.Children.First(c => "S2_" + c.Value.OtauId == childConnection.inputOtauId);
                result.Children.Add(pair.Key, pair.Value);

                result.FullPortCount += pair.Value.OwnPortCount - 1;
            }

            return result;
        }
    }
}
