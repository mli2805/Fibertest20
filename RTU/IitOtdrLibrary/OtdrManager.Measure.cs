using System;
using System.Threading;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Dto;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.IitOtdrLibrary
{
    public partial class OtdrManager
    {
        public ErrorCode MeasureWithBase(byte[] buffer, Charon activeChild)
        {
            var result = ErrorCode.Error;

            // allocate memory inside c++ library
            // put there base sor data
            // return pointer to that data, than you can say c++ code to use this data
            var baseSorData = IitOtdr.SetSorData(buffer);
            if (IitOtdr.SetMeasurementParametersFromSor(ref baseSorData))
            {
                IitOtdr.ForceLmaxNs(IitOtdr.ConvertLmaxOwtToNs(buffer));
                result = Measure(activeChild);
            }

            // free memory where was base sor data
            IitOtdr.FreeSorDataMemory(baseSorData);
            return result;
        }

        public ErrorCode DoManualMeasurement(bool shouldForceLmax, Charon activeChild)
        {
            if (shouldForceLmax)
                IitOtdr.ForceLmaxNs(IitOtdr.ConvertLmaxKmToNs());

            return Measure(activeChild);
        }

        private IntPtr _sorData = IntPtr.Zero;

        /// <summary>
        /// after Measure() use GetLastSorData() to obtain measurement result
        /// </summary>
        /// <returns></returns>
        private ErrorCode Measure(Charon activeChild)
        {
            _rtuLogger.AppendLine("Measurement begin.");

            if (!IitOtdr.PrepareMeasurement(true))
            {
                _rtuLogger.AppendLine("Prepare measurement error!");
                return ErrorCode.MeasurementPreparationError;
            }

            activeChild?.ShowMessageMeasurementPort();

            var result = MeasureSteps();

            activeChild?.ShowOnDisplayMessageReady();

            return result;
        }

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private ErrorCode MeasureSteps()
        {
            try
            {
                bool hasMoreSteps;
                do
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        IitOtdr.StopMeasurement(true);
                        _rtuLogger.AppendLine("Measurement interrupted.");
                        return ErrorCode.MeasurementInterrupted;
                    }

                    hasMoreSteps = IitOtdr.DoMeasurementStep(ref _sorData);
                } while (hasMoreSteps);

            }
            catch (Exception e)
            {
                _rtuLogger.AppendLine(e.Message);
                return ErrorCode.MeasurementError;
            }

            _rtuLogger.AppendLine("Measurement ended normally.");
            return ErrorCode.MeasurementEndedNormally;
        }

        public void InterruptMeasurement()
        {
            _cancellationTokenSource.Cancel();
        }

        public byte[] GetLastSorDataBuffer()
        {
            int bufferLength = IitOtdr.GetSorDataSize(_sorData);
            if (bufferLength == -1)
            {
                _rtuLogger.AppendLine("_sorData is null");
                return null;
            }
            byte[] buffer = new byte[bufferLength];

            var size = IitOtdr.GetSordata(_sorData, buffer, bufferLength);
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
            var measIntPtr = IitOtdr.SetSorData(measBytes);
            if (!IitOtdr.MakeAutoAnalysis(ref measIntPtr))
            {
                _rtuLogger.AppendLine("ApplyAutoAnalysis error.");
                return null;
            }
            var size = IitOtdr.GetSorDataSize(measIntPtr);
            byte[] resultBytes = new byte[size];
            IitOtdr.GetSordata(measIntPtr, resultBytes, size);

            IitOtdr.FreeSorDataMemory(measIntPtr);
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
