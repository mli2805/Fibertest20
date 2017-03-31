using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        public List<Role> Roles { get; set; }

        public UsersListViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;

            Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();
            Users = new ObservableCollection<User>()
            {
                new User() {Name = @"root1", Role = Role.Root, Password = @"root", IsEmailActive = false, Email = ""},
                new User() {Name = @"operator1", Role = Role.Operator, Password = @"operator", IsEmailActive = true, Email = @"op123op@mail.ru"},
                new User() {Name = @"supervisor1", Role = Role.Supervisor, Password = @"supervisor", IsEmailActive = false, Email = ""},
            };
        }

        public void Close()
        {
            TryClose();
        }
    }
}
