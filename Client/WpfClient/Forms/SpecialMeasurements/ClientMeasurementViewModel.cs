using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class ClientMeasurementViewModel : Screen
    {
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private DoClientMeasurementDto _dto;

        public ClientMeasurementViewModel(IWcfServiceForClient c2DWcfManager, IWindowManager windowManager)
        {
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
        }

        public bool Initialize(Leaf parent, int portNumber)
        {
            RtuLeaf rtuLeaf = parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent;
            var otau = (IPortOwner)parent;
            var address = otau.OtauNetAddress;

            var vm = new OtdrParametersThroughServerSetterViewModel(rtuLeaf.TreeOfAcceptableMeasParams);
            IWindowManager windowManager = new WindowManager();
            windowManager.ShowDialog(vm);
            if (!vm.IsAnswerPositive)
                return false;

            _dto = new DoClientMeasurementDto()
            {
                RtuId = rtuLeaf.Id,
                OtauPortDto = new OtauPortDto()
                {
                    OtauIp = address.Ip4Address,
                    OtauTcpPort = address.Port,
                    IsPortOnMainCharon = rtuLeaf.OtauNetAddress.Equals(address),
                    OpticalPort = portNumber
                },
                SelectedMeasParams = vm.GetSelectedParameters(),
            };
            return true;
        }

        protected override async void OnViewLoaded(object view)
        {
            DisplayName = @"Measurement Client";

            var result = await _c2DWcfManager.DoClientMeasurementAsync(_dto);
            if (result.ReturnCode != ReturnCode.Ok)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, result.ExceptionMessage);
                _windowManager.ShowDialogWithAssignedOwner(vm);
                TryClose();
                return;
            }

            var filename = @"..\temp\meas.sor";
            SorData.Save(result.SorBytes, filename);
            System.Diagnostics.Process.Start(@"..\RftsReflect\Reflect.exe", filename);

            TryClose();
        }

        public void InterruptMeasurement()
        {
            TryClose();
        }
    }
}
