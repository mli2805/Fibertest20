using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Iit.Fibertest.TestBench
{
    public static class TreeExtensions
    {
        public static Leaf GetById(this ObservableCollection<Leaf> roots, Guid id)
        {
            foreach (var root in roots)
            {
                if (root.Id == id)
                    return root;

                var portOwner = root as IPortOwner;
                if (portOwner == null)
                    continue;

                var result = GetById(portOwner.ChildrenImpresario.Children, id);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static int TraceCount(this ObservableCollection<Leaf> roots)
        {
            return roots.Sum(r=> ((RtuLeaf)r).TraceCount);
        }

        public static int PortCount(this ObservableCollection<Leaf> roots)
        {
            return roots.Sum(r=>((RtuLeaf)r).FullPortCount);
        }
    }


    
}
