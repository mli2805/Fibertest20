using System;
using System.Linq;
using Iit.Fibertest.DirectCharonLibrary;
using Iit.Fibertest.Utils35;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.IitOtdrLibrary
{
    public partial class OtdrManager
    {
        public bool MeasureWithBase(byte[] buffer)
        {
            var result = false;

            // allocate memory inside c++ library
            // put there base sor data
            // return pointer to that data, than you can say c++ code to use this data
            var baseSorData = IitOtdr.SetSorData(buffer);
            if (IitOtdr.SetMeasurementParametersFromSor(ref baseSorData))
            {
                IitOtdr.ForceLmaxNs(IitOtdr.ConvertLmaxOwtToNs(buffer));
                result = Measure();
            }

            // free memory where was base sor data
            IitOtdr.FreeSorDataMemory(baseSorData);
            return result;
        }

        public bool DoManualMeasurement(bool shouldForceLmax)
        {
            if (shouldForceLmax)
                IitOtdr.ForceLmaxNs(IitOtdr.ConvertLmaxKmToNs());

            return Measure();
        }

        private readonly object _lockObj = new object();
        private bool _isMeasurementCanceled;
        private IntPtr _sorData = IntPtr.Zero;

        /// <summary>
        /// after Measure() use GetLastSorData() to obtain measurement result
        /// </summary>
        /// <returns></returns>
        private bool Measure()
        {
            _rtuLogger.AppendLine("Measurement begin.");
            lock (_lockObj)
            {
                _isMeasurementCanceled = false;
            }

            // it should be done in outer scope (something like RtuManager, which has its own MainCharon)
            var mainCharon = new Charon(new NetAddress(_ipAddress, 23), _iniFile, _rtuLogger);
            mainCharon.Initialize();
            NetAddress activeCharonAddress;
            int activePort;
            if (!mainCharon.GetExtendedActivePort(out activeCharonAddress, out activePort))
            {
                _rtuLogger.AppendLine("Can't get active port");
                return false;
            }

            Charon activeCharon = null;
            if (!activeCharonAddress.Equals(mainCharon.NetAddress))
            {
                activeCharon = mainCharon.Children.Values.First(c => c.NetAddress.Equals(activeCharonAddress));
                activeCharon.ShowMessageMeasurementPort();
            }
            // end of RtuManager block of code


            if (!IitOtdr.PrepareMeasurement(true))
            {
                _rtuLogger.AppendLine("Prepare measurement error!");
                return false;
            }


            try
            {
                bool hasMoreSteps;
                do
                {
                    lock (_lockObj)
                    {
                        if (_isMeasurementCanceled)
                        {
                            IitOtdr.StopMeasurement(true);
                            _rtuLogger.AppendLine("Measurement interrupted.");
                            break;
                        }
                    }

                    hasMoreSteps = IitOtdr.DoMeasurementStep(ref _sorData);
                }
                while (hasMoreSteps);
            }
            catch (Exception e)
            {
                _rtuLogger.AppendLine(e.Message);
                return false;
            }

            _rtuLogger.AppendLine("Measurement end.");

            // it should be done in outer scope (something like RtuManager, which has its own MainCharon)
            activeCharon?.ShowMessageReady();

            return true;
        }

        public void InterruptMeasurement()
        {
            lock (_lockObj)
            {
                _isMeasurementCanceled = true;
            }
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
            _rtuLogger.AppendLine("Measurement result received.");
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
