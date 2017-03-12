﻿using System;
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

                var result = GetById(portOwner.ChildrenProvider.Children, id);
                if (result != null)
                    return result;
            }
            return null;
        }
    }

    public static class TreeStatistics
    {
        public static int TracesCount(this ObservableCollection<Leaf> roots)
        {
            return roots.Select(r => ((RtuLeaf) r).RtuTraceCount()).Sum();
        }

        private static int RtuTraceCount(this RtuLeaf rtuLeaf)
        {
            return 1;
        }
        public static int PortCount(this ObservableCollection<Leaf> roots)
        {
            return roots.Select(r=>((RtuLeaf)r).FullPortCount).Sum();
        }
    }

    public static class OtauExtentions
    {
        
    }
}
