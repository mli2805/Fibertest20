using System;
using System.Threading;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.IitOtdrLibrary
{
    public partial class OtdrManager
    {
        public ReturnCode MeasureWithBase(byte[] buffer, Charon activeChild)
        {
            var result = ReturnCode.Error;

            // allocate memory inside c++ library
            // put there base sor data
            // return pointer to that data, than you can say c++ code to use this data
            var baseSorData = InterOpWrapper.SetSorData(buffer);
            if (InterOpWrapper.SetMeasurementParametersFromSor(ref baseSorData))
            {
                InterOpWrapper.ForceLmaxNs(InterOpWrapper.ConvertLmaxOwtToNs(buffer));
                result = Measure(activeChild);
            }

            // free memory where was base sor data
            InterOpWrapper.FreeSorDataMemory(baseSorData);
            return result;
        }

        public ReturnCode DoManualMeasurement(bool shouldForceLmax, Charon activeChild)
        {
            if (shouldForceLmax)
                InterOpWrapper.ForceLmaxNs(InterOpWrapper.ConvertLmaxKmToNs());

            return Measure(activeChild);
        }

        private IntPtr _sorData = IntPtr.Zero;

        /// <summary>
        /// after Measure() use GetLastSorData() to obtain measurement result
        /// </summary>
        /// <returns></returns>
        private ReturnCode Measure(Charon activeChild)
        {
            _rtuLogger.AppendLine("Measurement begin.");

            if (!InterOpWrapper.PrepareMeasurement(true))
            {
                _rtuLogger.AppendLine("Prepare measurement error!");
                return ReturnCode.MeasurementPreparationError;
            }

            activeChild?.ShowMessageMeasurementPort();

            var result = MeasureSteps();

            activeChild?.ShowOnDisplayMessageReady();

            return result;
        }

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private ReturnCode MeasureSteps()
        {
            try
            {
                bool hasMoreSteps;
                do
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        InterOpWrapper.StopMeasurement(true);
                        _rtuLogger.AppendLine("Measurement interrupted.");
                        return ReturnCode.MeasurementInterrupted;
                    }

                    hasMoreSteps = InterOpWrapper.DoMeasurementStep(ref _sorData);
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

        public void InterruptMeasurement()
        {
            _cancellationTokenSource.Cancel();
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
            //            _rtuLogger.AppendLine("Measurement result received.");
            return buffer;
        }

        public byte[] ApplyAutoAnalysis(byte[] measBytes)
        {
            var measIntPtr = InterOpWrapper.SetSorData(measBytes);
            if (!InterOpWrapper.MakeAutoAnalysis(ref measIntPtr))
            {
                _rtuLogger.AppendLine("ApplyAutoAnalysis error.");
                return null;
            }
            var size = InterOpWrapper.GetSorDataSize(measIntPtr);
            byte[] resultBytes = new byte[size];
            InterOpWrapper.GetSordata(measIntPtr, resultBytes, size);

            InterOpWrapper.FreeSorDataMemory(measIntPtr);
            return resultBytes;
        }

        public OtdrDataKnownBlocks ApplyFilter(byte[] sorBytes, bool isFilterOn)
        {
            var sorData = SorData.FromBytes(sorBytes);
            sorData.IitParameters.Parameters = (IitBlockParameters)SetBitFlagInParameters((int)sorData.IitParameters.Parameters, IitBlockParameters.Filter, isFilterOn);
            return sorData;
        }

        public bool IsFilterOnInBase(byte[] sorBytes)
        {
            var sorData = SorData.FromBytes(sorBytes);
            return (sorData.IitParameters.Parameters & IitBlockParameters.Filter) != 0;
        }

        private int SetBitFlagInParameters(int parameters, IitBlockParameters parameter, bool flag)
        {
            return flag
                ? parameters | (int)parameter
                : parameters & (65535 ^ (int)parameter);
        }
    }
}
