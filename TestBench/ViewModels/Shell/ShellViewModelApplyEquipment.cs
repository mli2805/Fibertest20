using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
   
    public partial class ShellViewModel
    {
        private UpdateEquipment PrepareCommand(UpdateEquipment request)
        {
            var cmd = request;
            return cmd;
        }

        private RemoveEquipment PrepareCommand(RemoveEquipment request)
        {
            var cmd = new RemoveEquipment() {Id = request.Id};
            return cmd;
        }

    }
}
