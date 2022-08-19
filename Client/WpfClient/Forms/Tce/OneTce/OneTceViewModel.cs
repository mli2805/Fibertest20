using System;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WpfCommonViews;

namespace Iit.Fibertest.Client
{
    public class OneTceViewModel : Screen
    {
        private readonly CurrentUser _currentUser;
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        private readonly Model _readModel;
        private readonly IWindowManager _windowManager;
        private readonly TceReportProvider _tceReportProvider;
        private TceS _tceInWork;
        public TceInfoViewModel TceInfoViewModel { get; set; } = new TceInfoViewModel();
        public TceSlotsViewModel TceSlotsViewModel { get; set; } = new TceSlotsViewModel();

        public bool IsSaveEnabled => !string.IsNullOrEmpty(TceInfoViewModel.Title) && _currentUser.Role <= Role.Root;

        public OneTceViewModel(CurrentUser currentUser, IWcfServiceDesktopC2D c2DWcfManager, 
            Model readModel, IWindowManager windowManager, TceReportProvider tceReportProvider)
        {
            _currentUser = currentUser;
            _c2DWcfManager = c2DWcfManager;
            _readModel = readModel;
            _windowManager = windowManager;
            _tceReportProvider = tceReportProvider;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Settings;
        }
        public void Initialize(TceS tce)
        {
            _tceInWork = tce;
            TceInfoViewModel.Initialize(tce, _currentUser.Role <= Role.Root);
            TceInfoViewModel.PropertyChanged += TceInfoViewModel_PropertyChanged;
            TceSlotsViewModel.Initialize(_readModel, tce, IsTraceLinked, _currentUser.Role <= Role.Root);
        }

        private void TceInfoViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"Title")
                NotifyOfPropertyChange(nameof(IsSaveEnabled));
        }

        private bool IsTraceLinked(Trace trace)
        {
            var relation = _readModel.GponPortRelations.FirstOrDefault(r => r.TraceId == trace.TraceId);
            if (relation != null && relation.TceId != _tceInWork.Id)
                return true;

            return TceSlotsViewModel.Slots.Any(s => s.Gpons.Any(g => g.GponInWork.Trace?.TraceId == trace.TraceId));

            // не дает изменить связь если она существовала (т.к. у трассы уже выставлен флаг и не изменится пока не сохранишь удаление)
            //return trace.TraceToTceLinkState != TraceToTceLinkState.NoLink
            //       || TceSlotsViewModel.Slots.Any(s => s.Gpons.Any(g => g.GponInWork.Trace?.TraceId == trace.TraceId));
        }

        public void ExportToPdf()
        {
            var report = _tceReportProvider.Create(_tceInWork);
            if (report == null) return;

            PdfExposer.Show(report, $@"TceReport{DateTime.Now:yyyyMMddHHmmss}.pdf", _windowManager);
        }

        public async void ButtonSave()
        {
            await Save();
        }
        public async void ButtonSaveAndClose()
        {
            if (await Save())
                TryClose();
        }

        private async Task<bool> Save()
        {
            var cmd = new AddOrUpdateTceWithRelations
            {
                Id = _tceInWork.Id,
                Title = TceInfoViewModel.Title,
                TceTypeStruct = _tceInWork.TceTypeStruct,
                Ip = TceInfoViewModel.Ip4InputViewModel.GetString(),
                Slots = TceSlotsViewModel.Slots.Select(s=>s.GetTceSlot()).ToList(),
                ProcessSnmpTraps = TceInfoViewModel.ProcessSnmpTraps,
                Comment = TceInfoViewModel.Comment,
                AllRelationsOfTce = TceSlotsViewModel.Slots.SelectMany(s => s.GetGponPortsRelations()).ToList(),
            };

            var result = await _c2DWcfManager.SendCommandAsObj(cmd);
            if (result != null)
            {
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, result));
            }
            return string.IsNullOrEmpty(result);
        }

        public void Cancel()
        {
            TryClose();
        }

    }
}
