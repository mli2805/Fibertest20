using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using AutoMapper;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;
using Iit.Fibertest.WpfCommonViews;


namespace Iit.Fibertest.Client
{
    public class RtuUpdateViewModel : Screen, IDataErrorInfo
    {
        public Guid RtuId;
        private Rtu _originalRtu;
        private Node _originalNode;
        private readonly ILifetimeScope _globalScope;
        private readonly Model _readModel;
        private readonly GraphReadModel _graphReadModel;
        private readonly TabulatorViewModel _tabulatorViewModel;
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private bool _isInCreationMode;

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                NotifyOfPropertyChange();
            }
        }

        private string _comment;
        public string Comment
        {
            get => _comment;
            set
            {
                if (value == _comment) return;
                _comment = value;
                NotifyOfPropertyChange();
            }
        }

        private bool _isButtonSaveEnabled;
        public bool IsButtonSaveEnabled
        {
            get => _isButtonSaveEnabled;
            set
            {
                if (value == _isButtonSaveEnabled) return;
                _isButtonSaveEnabled = value;
                NotifyOfPropertyChange();
            }
        }
        public GpsInputViewModel GpsInputViewModel { get; set; }
        public bool IsEditEnabled { get; set; }


        public RtuUpdateViewModel(ILifetimeScope globalScope, CurrentUser currentUser,
            Model readModel, GraphReadModel graphReadModel, TabulatorViewModel tabulatorViewModel,
            IWcfServiceForClient c2DWcfManager, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _graphReadModel = graphReadModel;
            _tabulatorViewModel = tabulatorViewModel;
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            IsEditEnabled = currentUser.Role <= Role.Root;
        }

        public void Initialize(Guid rtuId)
        {
            RtuId = rtuId;
            _originalRtu = _readModel.Rtus.First(r => r.Id == RtuId);

            _originalNode = _readModel.Nodes.First(n => n.NodeId == _originalRtu.NodeId);
            GpsInputViewModel = _globalScope.Resolve<GpsInputViewModel>();
            GpsInputViewModel.Initialize(_originalNode.Position, IsEditEnabled);

            Title = _originalRtu.Title;
            Comment = _originalRtu.Comment;
        }

        public void Initialize(RequestAddRtuAtGpsLocation request)
        {
            _isInCreationMode = true;
            var nodeId = Guid.NewGuid();
            _originalNode = new Node() { NodeId = nodeId, Position = new PointLatLng(request.Latitude, request.Longitude) };
            RtuId = Guid.NewGuid();
            _originalRtu = new Rtu() { Id = RtuId, NodeId = nodeId };

            GpsInputViewModel = _globalScope.Resolve<GpsInputViewModel>();
            GpsInputViewModel.Initialize(_originalNode.Position, IsEditEnabled);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_RTU_Information;
        }

        public async void Save()
        {
            if (_isInCreationMode)
                await CreateRtu();
            else
                await UpdateRtu();
            TryClose();
        }

        private async Task CreateRtu()
        {
            var cmd = new AddRtuAtGpsLocation()
            {
                Id = _originalRtu.Id,
                NodeId = _originalNode.NodeId,
                Latitude = GpsInputViewModel.OneCoorViewModelLatitude.StringsToValue(),
                Longitude = GpsInputViewModel.OneCoorViewModelLongitude.StringsToValue(),
                Title = Title,
                Comment = Comment,
            };
            var result = await _c2DWcfManager.SendCommandAsObj(cmd);
            if (result != null)
            {
                var mb = new MyMessageBoxViewModel(MessageType.Error, result);
                _windowManager.ShowDialogWithAssignedOwner(mb);
                TryClose();
            }
        }

        private async Task UpdateRtu()
        {
            IMapper mapper =
                new MapperConfiguration(cfg => cfg.AddProfile<MappingViewModelToCommand>()).CreateMapper();
            UpdateRtu cmd = mapper.Map<UpdateRtu>(this);
            cmd.Position = new PointLatLng(GpsInputViewModel.OneCoorViewModelLatitude.StringsToValue(), GpsInputViewModel.OneCoorViewModelLongitude.StringsToValue());
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        public void PreView()
        {
            var nodeVm = _graphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _originalNode.NodeId);
            if (nodeVm == null) return;

            nodeVm.Position = new PointLatLng(GpsInputViewModel.OneCoorViewModelLatitude.StringsToValue(),
                GpsInputViewModel.OneCoorViewModelLongitude.StringsToValue());

            _graphReadModel.PlacePointIntoScreenCenter(nodeVm.Position);
            if (_tabulatorViewModel.SelectedTabIndex != 3)
                _tabulatorViewModel.SelectedTabIndex = 3;
        }

        public void Cancel()
        {
            var nodeVm = _graphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == _originalNode.NodeId);
            if (nodeVm != null)
            {
                nodeVm.Position = _originalNode.Position;
            }

            TryClose();
        }

        public string this[string columnName]
        {
            get
            {
                var errorMessage = string.Empty;
                switch (columnName)
                {
                    case "Title":
                        if (string.IsNullOrEmpty(Title))
                            errorMessage = Resources.SID_Title_is_required;
                        if (_readModel.Rtus.Any(n => n.Title == Title && n.Id != _originalRtu.Id))
                            errorMessage = Resources.SID_There_is_a_rtu_with_the_same_title;
                        IsButtonSaveEnabled = IsEditEnabled && errorMessage == string.Empty;
                        break;
                }
                return errorMessage;
            }
        }

        public string Error { get; set; }
    }


}
