using System.Collections.Generic;
using System.Windows;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TceRelationInfo
    {
        public Visibility Visibility { get; set; } = Visibility.Collapsed;

        public List<TceS> Tces { get; set; }
    }
}
