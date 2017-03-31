using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class UserListViewModel : Screen
    {
        private readonly IWindowManager _windowManager;
        private User _selectedUser;
        public ObservableCollection<User> Users { get; set; }

        public User SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                _selectedUser = value; 
                NotifyOfPropertyChange(nameof(IsRemovable));

            }
        }

        public bool IsRemovable => SelectedUser.Role != Role.Root;


        public static List<Role> Roles { get; set; }

        public UserListViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;

            Roles = Enum.GetValues(typeof(Role)).Cast<Role>().ToList();
            Users = new ObservableCollection<User>()
            {
                new User() {Name = @"root1", Role = Role.Root, Password = @"root", IsEmailActivated = false, Email = @"main_abonent@yandex.ru"},
                new User() {Name = @"operator1", Role = Role.Operator, Password = @"operator", IsEmailActivated = true, Email = @"op123op@mail.ru"},
                new User() {Name = @"supervisor1", Role = Role.Supervisor, Password = @"supervisor", IsEmailActivated = false, Email = ""},
            };
        }
        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_User_list;
        }

        public void Close()
        {
            TryClose();
        }
    }
}
