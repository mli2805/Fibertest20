using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public class RtuUpdateViewModel : Screen
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public RtuUpdateViewModel()
        {
            Latitude = 53.5;
            Longitude = 28.5;
        }

        protected override void OnViewLoaded(object view)
        {
        }
    }


}
