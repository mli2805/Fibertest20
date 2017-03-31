using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public enum Role
    {
        Root = 2,
        Operator = 3,
        Supervisor = 4,
        Superclient = 5,
    }
    public class User
    {
        public string Name { get; set; }
        public Role Role { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }
        public bool IsEmailActive { get; set; }
    }
    public class UsersListViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        public ObservableCollection<User> Users { get; set; }

        public UsersListViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
            Users = new ObservableCollection<User>()
            {
                new User() {Name = @"root", Role = Role.Root, Password = @"root", IsEmailActive = false, Email = ""},
                new User() {Name = @"operator", Role = Role.Root, Password = @"operator", IsEmailActive = false, Email = ""},
                new User() {Name = @"supervisor", Role = Role.Root, Password = @"supervisor", IsEmailActive = false, Email = ""},
            };
        }

        public void Close()
        {
            TryClose();
        }
    }
}
