using System;
using System.Linq;

namespace Iit.Fibertest.Graph
{
    public static class PathFinderExperiment
    {
        public static void Run(ReadModel readModel)
        {
            var zz = new Node() {Id = Guid.NewGuid(), Title = "zz"};
            readModel.Nodes.Add(zz);
            var z2 = new Node() {Id = Guid.NewGuid(), Title = "z2"};
            readModel.Nodes.Add(z2);
            readModel.Fibers.Add(new Fiber() {Id = new Guid(), Node1 = zz.Id, Node2 = z2.Id});

            var a0 = new Node() {Id = Guid.NewGuid(), Title = "a0"};
            readModel.Nodes.Add(a0);
            var b0 = new Node() {Id = Guid.NewGuid(), Title = "b0"};
            readModel.Nodes.Add(b0);
            var b1 = new Node() {Id = Guid.NewGuid(), Title = "b1"};
            readModel.Nodes.Add(b1);
            var b2 = new Node() {Id = Guid.NewGuid(), Title = "b2"};
            readModel.Nodes.Add(b2);
            var c0 = new Node() {Id = Guid.NewGuid(), Title = "c0"};
            readModel.Nodes.Add(c0);
            var c1 = new Node() {Id = Guid.NewGuid(), Title = "c1"};
            readModel.Nodes.Add(c1);
            var c2 = new Node() {Id = Guid.NewGuid(), Title = "c2"};
            readModel.Nodes.Add(c2);
            var d0 = new Node() {Id = Guid.NewGuid(), Title = "d0"};
            readModel.Nodes.Add(d0);
            var d1 = new Node() {Id = Guid.NewGuid(), Title = "d1"};
            readModel.Nodes.Add(d1);
            var d2 = new Node() {Id = Guid.NewGuid(), Title = "d2"};
            readModel.Nodes.Add(d2);
            var e0 = new Node() {Id = Guid.NewGuid(), Title = "e0"};
            readModel.Nodes.Add(e0);
            var e1 = new Node() {Id = Guid.NewGuid(), Title = "e1"};
            readModel.Nodes.Add(e1);
            var e2 = new Node() {Id = Guid.NewGuid(), Title = "e2"};
            readModel.Nodes.Add(e2);
            var nn = new Node() {Id = Guid.NewGuid(), Title = "nn"};
            readModel.Nodes.Add(nn);


            readModel.Fibers.Add(new Fiber() {Id = new Guid(), Node1 = a0.Id, Node2 = b0.Id});
            readModel.Fibers.Add(new Fiber() {Id = new Guid(), Node1 = a0.Id, Node2 = b1.Id});
            readModel.Fibers.Add(new Fiber() {Id = new Guid(), Node1 = a0.Id, Node2 = b2.Id});

            readModel.Fibers.Add(new Fiber() {Id = new Guid(), Node1 = c0.Id, Node2 = b0.Id});
            readModel.Fibers.Add(new Fiber() {Id = new Guid(), Node1 = c1.Id, Node2 = b1.Id});
            readModel.Fibers.Add(new Fiber() {Id = new Guid(), Node1 = c2.Id, Node2 = b2.Id});

            readModel.Fibers.Add(new Fiber() {Id = new Guid(), Node1 = c0.Id, Node2 = d0.Id});
            readModel.Fibers.Add(new Fiber() {Id = new Guid(), Node1 = c1.Id, Node2 = d1.Id});
            readModel.Fibers.Add(new Fiber() {Id = new Guid(), Node1 = c2.Id, Node2 = d2.Id});

            readModel.Fibers.Add(new Fiber() {Id = new Guid(), Node1 = e0.Id, Node2 = d0.Id});
            readModel.Fibers.Add(new Fiber() {Id = new Guid(), Node1 = e1.Id, Node2 = d1.Id});
            readModel.Fibers.Add(new Fiber() {Id = new Guid(), Node1 = e2.Id, Node2 = d2.Id});

            readModel.Fibers.Add(new Fiber() {Id = new Guid(), Node1 = e0.Id, Node2 = e1.Id});
            readModel.Fibers.Add(new Fiber() {Id = new Guid(), Node1 = e1.Id, Node2 = nn.Id});
            readModel.Fibers.Add(new Fiber() {Id = new Guid(), Node1 = e2.Id, Node2 = nn.Id});

            var pf = new PathFinder(readModel);
            var tr = pf.FindPath(a0.Id, nn.Id);

            if (tr == null)
                Console.WriteLine("Path not found");
            else
                foreach (var guid in tr)
                {
                    Console.WriteLine($"{readModel.Nodes.Single(n => n.Id == guid).Title}");
                }

        }
    }
}
