using System;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;

namespace Iit.Fibertest.Client
{
    public class TceViewModel : Screen
    {
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private bool _isInCreationMode;

        public Guid TceId { get; set; }

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

        public string Comment { get; set; }

        public Ip4InputViewModel Ip4InputViewModel { get; set; }
      
        public string[] TceTypes { get; set; }
        public TceType SelectedTceType { get; set; }

        public TceViewModel(IWcfServiceDesktopC2D c2DWcfManager)
        {
            _c2DWcfManager = c2DWcfManager;
            TceTypes = Enum.GetNames(typeof(TceType));
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _isInCreationMode ? Resources.SID_Add_Equipment : Resources.SID_Update_equipment;
        }

        public void Initialize(Tce selectedTce)
        {
            _isInCreationMode = false;
            TceId = selectedTce.Id;
            Title = selectedTce.Title;
            SelectedTceType = selectedTce.TceType;
            Ip4InputViewModel = new Ip4InputViewModel(selectedTce.Ip);
            Comment = selectedTce.Comment;
        }

        public void Initialize()
        {
            _isInCreationMode = true;
            TceId = Guid.NewGuid();
            Ip4InputViewModel = new Ip4InputViewModel(@"0.0.0.0");
        }

        public async void Save()
        {
            var cmd = new AddOrUpdateTce()
            {
                Id = TceId,
                Title = Title,
                TceType = SelectedTceType,
                Ip = Ip4InputViewModel.GetString(),
                Comment = Comment,
            };
            var res = await _c2DWcfManager.SendCommandAsObj(cmd);
            TryClose(string.IsNullOrEmpty(res));
        }

        public void Cancel()
        {
            TryClose(false);
        }

    }
}
