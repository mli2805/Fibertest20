using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TceInfoViewModel : PropertyChangedBase
    {
        public string Title { get; set; }

        public Ip4InputViewModel Ip4InputViewModel { get; set; }

        public string Comment { get; set; }

        public TceS Tce { get; set; }

        public void Initialize(TceS tce)
        {
            Tce = tce;

            Title = tce.Title;
            Ip4InputViewModel = new Ip4InputViewModel(tce.Ip);
            Comment = tce.Comment;
        }
    }
}
