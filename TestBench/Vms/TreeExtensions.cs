using System;
using System.Collections.ObjectModel;

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
                var result = GetById(root.Children, id);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
