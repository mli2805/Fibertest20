using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Iit.Fibertest.Graph;
using TreeVision.Properties;

namespace TreeVision
{
    public enum LeafType
    {
        DataCenter,
        Rtu,
        Bop,
        Trace
    }
    public class Leaf : ITreeViewItemModel
    {
        public string Title { get; set; }

        public Leaf Parent { get; set; }
        public ObservableCollection<Leaf> Children { get; set; } = new ObservableCollection<Leaf>();

        public Guid Id { get; set; }
        public LeafType LeafType { get; set; }

        public FiberState State { get; set; }
        public Brush Color { get; set; }
        public ImageSource Pic1 { get; set; }


        #region implementation of ITreeViewItemModel
        public string SelectedValuePath => Title;
        public string DisplayValuePath => Title;

        public bool IsExpanded { get; set; }
        public bool IsSelected { get; set; }

        private IEnumerable<Leaf> GetAscendingHierarchy()
        {
            var account = this;

            yield return account;
            while (account.Parent != null)
            {
                yield return account.Parent;
                account = account.Parent;
            }
        }
        public IEnumerable<ITreeViewItemModel> GetHierarchy()
        {
            return GetAscendingHierarchy().Reverse();
        }

        public IEnumerable<ITreeViewItemModel> GetChildren()
        {
            return Children;
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
