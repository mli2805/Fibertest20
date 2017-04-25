using Caliburn.Micro;

namespace Iit.Fibertest.Client
{
    public class LandmarkViewModel : Screen
    {
        public LandmarkViewModel()
        {
        }

        public void Initialize(Landmark landmark)
        {
            DisplayName = landmark.NodeTitle;
        }
    }
}
