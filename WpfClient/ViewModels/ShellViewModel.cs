using System;
using System.Linq;
using System.Windows;
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

        public void LaunchAddEquipmentView()
        {
            var node = new Node();
            node.Id = Guid.NewGuid();
            _readModel.Nodes.Add(node);

            var windowManager = IoC.Get<IWindowManager>();
            var addEquipmentViewModel = new AddEquipmentViewModel(node.Id, _readModel, _aggregate);
            windowManager.ShowDialog(addEquipmentViewModel);
        }

        public void DefineTrace()
        {
            Guid rtuNodeId;
            Guid lastNodeId;
            var ee = new PathFinderExperiment(_readModel);
            ee.PopulateReadModelForExperiment(out rtuNodeId, out lastNodeId);


            var path = new PathFinder(_readModel).FindPath(rtuNodeId, lastNodeId).ToList();

            if (path.Count == 0)
                MessageBox.Show("Path couldn't be found");
            else
            {
                foreach (var guid in path)
                {
                    Console.WriteLine($"{_readModel.Nodes.Single(n => n.Id == guid).Title}");
                }
                var windowManager = IoC.Get<IWindowManager>();
                var addEquipmentViewModel = new AddTraceViewModel(_readModel, _aggregate, path);
                windowManager.ShowDialog(addEquipmentViewModel);
            }

        }
    }
}