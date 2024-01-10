using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public static class RoleExt
    {
        public static bool IsWebPermitted(this Role role)
        {
            return role == Role.Developer || role == Role.Root ||
                   role == Role.WebOperator || role == Role.WebSupervisor;
        }

        public static bool IsSuperClientPermitted(this Role role)
        {
            return role == Role.Developer || role == Role.SuperClient;
        }

        public static bool IsDesktopPermitted(this Role role)
        {
            return role == Role.Developer || role == Role.Root ||
                   role == Role.Operator || role == Role.Supervisor;
        }
    }
}