using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public class UserViewModel : Screen
    {
        public User User { get; set; }
        private readonly bool _isCreateNewUserMode;

        public UserViewModel(bool IsCreateNewUserMode, User user)
        {
            User = user;
            _isCreateNewUserMode = IsCreateNewUserMode;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "User";
        }
    }
}
