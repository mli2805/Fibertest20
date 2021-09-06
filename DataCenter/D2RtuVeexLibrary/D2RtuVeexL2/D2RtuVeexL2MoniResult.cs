﻿using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        public async Task<MonitoringResultDto> GetTestLastMeasurement(DoubleAddress rtuAddresses, string testId, string type, bool isFast)
        {
            var kind = type == "monitoring_test_passed" ? "last_passed" : "last_failed";
            try
            {
                var completedTest = await _d2RtuVeexLayer1.GetCompletedTest(rtuAddresses, testId, kind);
                if (completedTest == null) return null;

                var sorBytes = await _d2RtuVeexLayer1.GetSorBytes(rtuAddresses, testId, kind);
                return new MonitoringResultDto()
                {
                    MeasurementResult = GetMeasurementResult(completedTest),
                    BaseRefType = isFast 
                        ? BaseRefType.Fast
                        : completedTest.indicesOfReferenceTraces[0] == 0 ? BaseRefType.Precise : BaseRefType.Additional,
                    SorBytes = sorBytes,
                };
            }
            catch (Exception)
            {
                return null;
            }

          
        }

        private MeasurementResult GetMeasurementResult(CompletedTest completedTest)
        {
            if (completedTest.result == "ok") return MeasurementResult.Success;

            if (completedTest.extendedResult == "otau_failed") return MeasurementResult.ToggleToPortFailed;
            if (completedTest.extendedResult == "otdr_failed") return MeasurementResult.HardwareProblem;

            return MeasurementResult.Success;
        }
    }
}
