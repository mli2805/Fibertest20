using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class TreeOfRtuViewModel : PropertyChangedBase
    {
        private readonly IMyLog _logFile;
        private readonly IWindowManager _windowManager;
        private readonly Model _readModel;
        private readonly ChildrenViews _childrenViews;
        public TreeOfRtuModel TreeOfRtuModel { get; set; }
        public FreePorts FreePorts { get; }

        private string _textToFind;

        public string TextToFind
        {
            get { return _textToFind; }
            set
            {
                if (value == _textToFind) return;
                _textToFind = value;
                NotifyOfPropertyChange();
                Find();
                NotifyOfPropertyChange(nameof(Found));
            }
        }

        private int _foundCounter;
        public string Found => _foundCounter == 0 ? "" : _foundCounter.ToString();

        public Visibility IsDev { get; set; }
        public TreeOfRtuViewModel(CurrentUser currentUser, IMyLog logFile, IWindowManager windowManager,
            Model readModel, TreeOfRtuModel treeOfRtuModel, FreePorts freePorts, 
            ChildrenViews childrenViews, EventArrivalNotifier eventArrivalNotifier)
        {
            IsDev = currentUser.Role == Role.Developer ? Visibility.Visible : Visibility.Collapsed;

            _logFile = logFile;
            _windowManager = windowManager;
            _readModel = readModel;
            _childrenViews = childrenViews;
            TreeOfRtuModel = treeOfRtuModel;
            TreeOfRtuModel.RefreshStatistics();

            FreePorts = freePorts;
            FreePorts.AreVisible = true;

            eventArrivalNotifier.PropertyChanged += _eventArrivalNotifier_PropertyChanged;
        }

        public void ChangeFreePortsVisibility()
        {
            FreePorts.AreVisible = !FreePorts.AreVisible;
        }

        private void _eventArrivalNotifier_PropertyChanged(object sender,
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            TreeOfRtuModel.RefreshStatistics();
        }

        public void CloseChildren()
        {
            _childrenViews.ShouldBeClosed = true;
        }

        public void CollapseAll()
        {
            foreach (var leaf in TreeOfRtuModel.Tree)
            {
                leaf.IsExpanded = false;
            }
        }

        private void Find()
        {
            ExtinguishRtus();
            if (string.IsNullOrEmpty(TextToFind)) return;

            foreach (var leaf in TreeOfRtuModel.Tree)
            {
                if (!(leaf is RtuLeaf rtuLeaf)) continue;
                FindLeaves(rtuLeaf);
            }
        }

        private void FindLeaves(IPortOwner portOwner)
        {
            var leaf = (Leaf)portOwner;
            if (leaf.Name.ToLower().Contains(TextToFind.ToLower()))
            {
                leaf.BackgroundBrush = Brushes.LightGoldenrodYellow;
                _foundCounter++;
            }
            foreach (var child in portOwner.ChildrenImpresario.Children)
            {
                if (child is IPortOwner subPortOwner)
                    FindLeaves(subPortOwner);
                if (child.Name.ToLower().Contains(TextToFind.ToLower()))
                {
                    child.BackgroundBrush = Brushes.LightGoldenrodYellow;
                    leaf.BackgroundBrush = Brushes.LightGoldenrodYellow;
                    _foundCounter++;
                }
            }
        }

        public void ClearButton()
        {
            TextToFind = "";
            ExtinguishRtus();
        }

        private void ExtinguishRtus()
        {
            _foundCounter = 0;
            foreach (var leaf in TreeOfRtuModel.Tree)
            {
                leaf.BackgroundBrush = Brushes.White;
                ExtinguishLeaves((IPortOwner)leaf);
            }
        }

        private void ExtinguishLeaves(IPortOwner portOwner)
        {
            foreach (var child in portOwner.ChildrenImpresario.EffectiveChildren)
            {
                if (child is IPortOwner subPortOwner)
                    ExtinguishLeaves(subPortOwner);
                child.BackgroundBrush = Brushes.White;
            }
        }

        public void LaunchTracesInDbReport()
        {
            foreach (var trace in _readModel.Traces)
            {
                if (!_readModel.TraceDevReport(trace, out List<string> content))
                {
                    var filename = $@"..\Reports\tdr-{trace.Title}.txt";
                    File.WriteAllLines(filename, content);
                    var vm = new MyMessageBoxViewModel(MessageType.Information, new List<string>()
                    {
                        @"Error(s) found on trace!",
                        "",
                        $@"Created report {filename}"
                    }, 0);
                    _windowManager.ShowDialogWithAssignedOwner(vm);
                }
                _logFile.AppendLine($@"checked: {trace.Title}");
            }
            _logFile.AppendLine(@"All traces are checked");
        }

    }
}