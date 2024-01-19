using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public static class RtuInitializeModelExt
    {
        public static InitializeRtuDto CreateDto(this RtuInitializeModel fullModel, CurrentUser currentUser)
        {
            var rtuMaker = fullModel.MainChannelTestViewModel.NetAddressInputViewModel.Port == (int)TcpPorts.RtuListenTo
                ? RtuMaker.IIT
                : RtuMaker.VeEX;

            if (fullModel.IsReserveChannelEnabled && fullModel.ReserveChannelTestViewModel.NetAddressInputViewModel.Port == -1)
                fullModel.ReserveChannelTestViewModel.NetAddressInputViewModel.Port = rtuMaker == RtuMaker.IIT
                    ? (int)TcpPorts.RtuListenTo
                : (int)TcpPorts.RtuVeexListenTo;

            if (fullModel.MainChannelTestViewModel.NetAddressInputViewModel.Port == -1)
                fullModel.MainChannelTestViewModel.NetAddressInputViewModel.Port = rtuMaker == RtuMaker.IIT
                    ? (int)TcpPorts.RtuListenTo
                    : (int)TcpPorts.RtuVeexListenTo;
            return new InitializeRtuDto()
            {
                ConnectionId = currentUser.ConnectionId,
                RtuMaker = rtuMaker, // it depends on which initialization button was pressed

                RtuId = fullModel.OriginalRtu.Id,
                Serial = fullModel.OriginalRtu.Serial, // properties after previous initialization (if it was)
                OwnPortCount = fullModel.OriginalRtu.OwnPortCount,
                MainVeexOtau = fullModel.OriginalRtu.MainVeexOtau,
                Children = fullModel.OriginalRtu.Children,

                RtuAddresses = new DoubleAddress()
                {
                    Main = fullModel.MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress(),
                    HasReserveAddress = fullModel.IsReserveChannelEnabled,
                    Reserve = fullModel.IsReserveChannelEnabled
                        ? fullModel.ReserveChannelTestViewModel.NetAddressInputViewModel.GetNetAddress()
                        : null,
                },
                IsFirstInitialization =
                    fullModel.OriginalRtu.OwnPortCount ==
                    0, // if it's first initialization for this RTU - monitoring should be stopped - in case it's running somehow
            };
        }

        public static async Task<bool> CheckConnectionBeforeInitialization(this RtuInitializeModel fullModel)
        {
            if (!fullModel.MainChannelTestViewModel.NetAddressInputViewModel.IsValidIpAddress())
            {
                fullModel.WindowManager.ShowDialogWithAssignedOwner(
                    new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Invalid_IP_address));
                return false;
            }
            if (!await fullModel.MainChannelTestViewModel.ExternalTest())
            {
                fullModel.WindowManager.ShowDialogWithAssignedOwner(
                    new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Cannot_establish_connection_with_RTU_));
                return false;
            }

            if (!fullModel.IsReserveChannelEnabled) return true;

            if (!fullModel.ReserveChannelTestViewModel.NetAddressInputViewModel.IsValidIpAddress())
            {
                fullModel.WindowManager.ShowDialogWithAssignedOwner(
                    new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Invalid_IP_address));
                return false;
            }
            if (await fullModel.ReserveChannelTestViewModel.ExternalTest()) return true;

            fullModel.WindowManager.ShowDialogWithAssignedOwner(
                new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Cannot_establish_connection_with_RTU_));
            return false;
        }
    }
}
