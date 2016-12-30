using System;
using System.Linq;
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
            var a0 = new Node() { Id = Guid.NewGuid(), Title = "a0" }; _readModel.Nodes.Add(a0);
            var b0 = new Node() { Id = Guid.NewGuid(), Title = "b0" }; _readModel.Nodes.Add(b0);
            var b1 = new Node() { Id = Guid.NewGuid(), Title = "b1" }; _readModel.Nodes.Add(b1);
            var b2 = new Node() { Id = Guid.NewGuid(), Title = "b2" }; _readModel.Nodes.Add(b2);
            var c0 = new Node() { Id = Guid.NewGuid(), Title = "c0" }; _readModel.Nodes.Add(c0);
            var c1 = new Node() { Id = Guid.NewGuid(), Title = "c1" }; _readModel.Nodes.Add(c1);
            var c2 = new Node() { Id = Guid.NewGuid(), Title = "c2" }; _readModel.Nodes.Add(c2);
            var d0 = new Node() { Id = Guid.NewGuid(), Title = "d0" }; _readModel.Nodes.Add(d0);
            var d1 = new Node() { Id = Guid.NewGuid(), Title = "d1" }; _readModel.Nodes.Add(d1);
            var d2 = new Node() { Id = Guid.NewGuid(), Title = "d2" }; _readModel.Nodes.Add(d2);
            var e0 = new Node() { Id = Guid.NewGuid(), Title = "e0" }; _readModel.Nodes.Add(e0);
            var e1 = new Node() { Id = Guid.NewGuid(), Title = "e1" }; _readModel.Nodes.Add(e1);
            var e2 = new Node() { Id = Guid.NewGuid(), Title = "e2" }; _readModel.Nodes.Add(e2);
            var nn = new Node() { Id = Guid.NewGuid(), Title = "nn" }; _readModel.Nodes.Add(nn);

            var zz = new Node() { Id = Guid.NewGuid(), Title = "zz" }; _readModel.Nodes.Add(zz);



            _readModel.Fibers.Add(new Fiber() { Id = new Guid(), Node1 = a0.Id, Node2 = b0.Id });
            _readModel.Fibers.Add(new Fiber() { Id = new Guid(), Node1 = a0.Id, Node2 = b1.Id });
            _readModel.Fibers.Add(new Fiber() { Id = new Guid(), Node1 = a0.Id, Node2 = b2.Id });

            _readModel.Fibers.Add(new Fiber() { Id = new Guid(), Node1 = c0.Id, Node2 = b0.Id });
            _readModel.Fibers.Add(new Fiber() { Id = new Guid(), Node1 = c1.Id, Node2 = b1.Id });
            _readModel.Fibers.Add(new Fiber() { Id = new Guid(), Node1 = c2.Id, Node2 = b2.Id });

            _readModel.Fibers.Add(new Fiber() { Id = new Guid(), Node1 = c0.Id, Node2 = d0.Id });
            _readModel.Fibers.Add(new Fiber() { Id = new Guid(), Node1 = c1.Id, Node2 = d1.Id });
            _readModel.Fibers.Add(new Fiber() { Id = new Guid(), Node1 = c2.Id, Node2 = d2.Id });

            _readModel.Fibers.Add(new Fiber() { Id = new Guid(), Node1 = e0.Id, Node2 = d0.Id });
            _readModel.Fibers.Add(new Fiber() { Id = new Guid(), Node1 = e1.Id, Node2 = d1.Id });
            _readModel.Fibers.Add(new Fiber() { Id = new Guid(), Node1 = e2.Id, Node2 = d2.Id });

            _readModel.Fibers.Add(new Fiber() { Id = new Guid(), Node1 = e0.Id, Node2 = e1.Id });
            _readModel.Fibers.Add(new Fiber() { Id = new Guid(), Node1 = e1.Id, Node2 = nn.Id });
            _readModel.Fibers.Add(new Fiber() { Id = new Guid(), Node1 = e2.Id, Node2 = nn.Id });

            var pf = new PathFinder(_readModel);
            var tr = pf.FindPath(a0.Id, nn.Id);

            if (tr == null)
                Console.WriteLine("Path not found");
            else
                foreach (var guid in tr)
                {
                    var title = _readModel.Nodes.Single(n => n.Id == guid).Title;
                    Console.WriteLine($"{title}");
                }
        }

    }
}