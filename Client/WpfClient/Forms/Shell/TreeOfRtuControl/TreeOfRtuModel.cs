﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class TreeOfRtuModel : PropertyChangedBase
    {

        public ObservableCollection<Leaf> Tree { get; set; } = new ObservableCollection<Leaf>();
        public FreePorts FreePorts { get; }

        public string Statistics =>
            string.Format(Resources.SID_Tree_statistics, Tree.Count,
                Tree.Sum(r => ((RtuLeaf)r).ChildrenImpresario.Children.Count(c => c is OtauLeaf)),
                Tree.Sum(r => ((RtuLeaf)r).FullPortCount), Tree.Sum(r => ((RtuLeaf)r).TraceCount), 
                (double)Tree.Sum(r => ((RtuLeaf)r).TraceCount) / Tree.Sum(r => ((RtuLeaf)r).FullPortCount) * 100);

        public TreeOfRtuModel(FreePorts freePorts)
        {
            FreePorts = freePorts;
            FreePorts.AreVisible = true;
        }

        public Leaf GetById(Guid id)
        {
            return GetById(Tree, id);
        }
        private Leaf GetById(ObservableCollection<Leaf> roots, Guid id)
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
      
    }
}
