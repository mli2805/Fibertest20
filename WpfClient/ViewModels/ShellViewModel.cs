using System;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public class ShellViewModel : Caliburn.Micro.PropertyChangedBase, IShell
    {
        private Aggregate _aggregate;
        private ReadModel _readModel;
        public ShellViewModel()
        {
            _aggregate = new Aggregate();
            _readModel = new ReadModel();
        }

        public void LaunchUpdateNodeView()
        {
            var node = new Node();
            node.Id = Guid.NewGuid();
            _readModel.Nodes.Add(node);


            var windowManager = IoC.Get<IWindowManager>();
            var updateNodeViewModel = new UpdateNodeViewModel(node.Id, _readModel, _aggregate);
            windowManager.ShowDialog(updateNodeViewModel);

        }
    }
}