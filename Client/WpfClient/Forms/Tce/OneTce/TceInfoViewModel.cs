using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TceInfoViewModel : PropertyChangedBase
    {
        public string Title { get; set; }

        public string TypeCode { get; private set; }

        public Ip4InputViewModel Ip4InputViewModel { get; set; }

        public string Comment { get; set; }

        public void Initialize(TceS tce)
        {
            Title = tce.Title;
            TypeCode = tce.TceTypeStruct.Code;
            Ip4InputViewModel = new Ip4InputViewModel(tce.Ip);
            Comment = tce.Comment;
        }
    }
}
