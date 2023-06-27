using System;
using System.Threading;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.IitOtdrLibrary
{
    public partial class OtdrManager
    {
        public ReturnCode MeasureWithBase(CancellationTokenSource cts, byte[] buffer, Charon bopCharonToShowPortOnDisplay)
        {
            ReturnCode result;

            // allocate memory inside c++ library
            // put there base sor data
            // return pointer to that data, than you can say c++ code to use this data
            var baseSorData = InterOpWrapper.SetSorData(buffer);
            if (InterOpWrapper.SetMeasurementParametersFromSor(ref baseSorData))
            {
                InterOpWrapper.ForceLmaxNs(InterOpWrapper.ConvertLmaxOwtToNs(buffer));
                result = Measure(cts, bopCharonToShowPortOnDisplay);
            }
            else
            {
                result = ReturnCode.MeasurementFailedToSetParametersFromBase;
            }

            // free memory where was base sor data
            InterOpWrapper.FreeSorDataMemory(baseSorData);
            return result;
        }

        public ReturnCode DoManualMeasurement(CancellationTokenSource cts, bool shouldForceLmax, Charon bopCharonToShowPortOnDisplay)
        {
            if (shouldForceLmax)
                InterOpWrapper.ForceLmaxNs(InterOpWrapper.ConvertLmaxKmToNs());

            return Measure(cts, bopCharonToShowPortOnDisplay);
        }

        private IntPtr _sorData = IntPtr.Zero;

        /// <summary>
        /// after Measure() use GetLastSorData() to obtain measurement result
        /// </summary>
        /// <returns></returns>
        private ReturnCode Measure(CancellationTokenSource cts, Charon bopCharonToShowPortOnDisplay)
        {
            _rtuLogger.AppendLine("Measurement begin.");

            if (!InterOpWrapper.PrepareMeasurement(true))
            {
                _rtuLogger.AppendLine("Prepare measurement error!");
                return ReturnCode.MeasurementPreparationError;
            }

            bopCharonToShowPortOnDisplay?.ShowMessageMeasurementPort();

            if (!InterOpWrapper.SetTuningApdMode(1))
            {
                _rtuLogger.AppendLine("Prepare measurement error!");
                return ReturnCode.MeasurementPreparationError;
            }

            var result = MeasureSteps(cts);

            bopCharonToShowPortOnDisplay?.ShowOnDisplayMessageReady();

            return result;
        }

        private ReturnCode MeasureSteps(CancellationTokenSource cts)
        {
            try
            {
                bool hasMoreSteps;
                int step = 0;
                do
                {
                    if (cts.IsCancellationRequested)
                    {
                        InterOpWrapper.StopMeasurement(true);
                        _rtuLogger.AppendLine("Measurement interrupted.");
                        return ReturnCode.MeasurementInterrupted;
                    }

                    var result = InterOpWrapper.DoMeasurementStep(ref _sorData);
                    if (((LogFile)_rtuLogger).LogLevel == 3)
                    {
                        var buffer = GetLastSorDataBuffer();
                        _rtuLogger.AppendLine($"MeasStep #{++step} returned {buffer.Length} bytes", 0, 3);
                    }

                    if (result != 0 && result != 10001)
                    {
                        _rtuLogger.AppendLine($"MeasStep returned {result}");
                        return ReturnCode.MeasurementError;
                    }
                    hasMoreSteps = result == 0;

                } while (hasMoreSteps);

            }
            catch (Exception e)
            {
                _rtuLogger.AppendLine(e.Message);
                return ReturnCode.MeasurementError;
            }

            _rtuLogger.AppendLine("Measurement ended normally.");
            return ReturnCode.MeasurementEndedNormally;
        }

        public byte[] GetLastSorDataBuffer()
        {
            int bufferLength = InterOpWrapper.GetSorDataSize(_sorData);
            if (bufferLength == -1)
            {
                _rtuLogger.AppendLine("_sorData is null");
                return null;
            }
            byte[] buffer = new byte[bufferLength];

            var size = InterOpWrapper.GetSordata(_sorData, buffer, bufferLength);
            if (size == -1)
            {
                _rtuLogger.AppendLine("Error in GetLastSorData");
                return null;
            }
            return buffer;
        }

        public byte[] ApplyAutoAnalysis(byte[] measBytes)
        {
            var measIntPtr = InterOpWrapper.SetSorData(measBytes);
            _rtuLogger.AppendLine("SetSorData done.", 4, 3);

            if (!InterOpWrapper.MakeAutoAnalysis(ref measIntPtr))
            {
                _rtuLogger.AppendLine("ApplyAutoAnalysis error.");
                return null;
            }
            _rtuLogger.AppendLine("ApplyAutoAnalysis done.", 4, 3);
            var size = InterOpWrapper.GetSorDataSize(measIntPtr);
            _rtuLogger.AppendLine("GetSorDataSize done.", 4, 3);
            byte[] resultBytes = new byte[size];
            InterOpWrapper.GetSordata(measIntPtr, resultBytes, size);
            _rtuLogger.AppendLine("GetSorData done.", 4, 3);
            InterOpWrapper.FreeSorDataMemory(measIntPtr);
            return resultBytes;
        }

        public byte[] Sf780_779(byte[] measBytes)
        {
            var measIntPtr = InterOpWrapper.SetSorData(measBytes);
            if (!InterOpWrapper.Analyze(ref measIntPtr, 1))
                return null;

            if (!InterOpWrapper.InsertIitEvents(ref measIntPtr))
                return null;

            var size = InterOpWrapper.GetSorDataSize(measIntPtr);
            byte[] resultBytes = new byte[size];
            InterOpWrapper.GetSordata(measIntPtr, resultBytes, size);
            InterOpWrapper.FreeSorDataMemory(measIntPtr);
            return resultBytes;
        }

        public byte[] Sf780(byte[] measBytes)
        {
            var measIntPtr = InterOpWrapper.SetSorData(measBytes);
            if (!InterOpWrapper.Analyze(ref measIntPtr, 1))
                return null;

            var size = InterOpWrapper.GetSorDataSize(measIntPtr);
            byte[] resultBytes = new byte[size];
            InterOpWrapper.GetSordata(measIntPtr, resultBytes, size);
            InterOpWrapper.FreeSorDataMemory(measIntPtr);
            return resultBytes;
        }

        public OtdrDataKnownBlocks ApplyFilter(byte[] sorBytes, bool isFilterOn)
        {
            var sorData = SorData.FromBytes(sorBytes);
            sorData.IitParameters.Parameters = SetBitFlagInParameters(sorData.IitParameters.Parameters, IitBlockParameters.Filter, isFilterOn);
            return sorData;
        }

        public bool IsFilterOnInBase(byte[] sorBytes)
        {
            var sorData = SorData.FromBytes(sorBytes);
            return (sorData.IitParameters.Parameters & IitBlockParameters.Filter) != 0;
        }

        private IitBlockParameters SetBitFlagInParameters(IitBlockParameters parameters, IitBlockParameters parameter, bool flag)
        {
            return flag
                ? parameters | parameter
                : parameters & ~parameter;
        }
    }
}
