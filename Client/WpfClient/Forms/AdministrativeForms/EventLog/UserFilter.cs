using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class UserFilter
    {
        public bool IsOn { get; set; }
        public User User { get; set; }

        public UserFilter() { IsOn = false; }
        public UserFilter(User user)
        {
            IsOn = true;
            User = user;
        }

        public override string ToString()
        {
            return IsOn ? User.Title : Resources.SID__no_filter_;
        }
    }
}