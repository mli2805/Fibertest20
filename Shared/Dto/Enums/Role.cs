namespace Iit.Fibertest.Dto
{
    public enum Role
    {
        System = 0,
        Developer = 1,
        Root = 2,
        Operator = 3,
        Supervisor = 4,
        Superclient = 5,

        NotificationReceiver = 9,

        WebOperator = 31,
        WebSupervisor = 41,
    }

    public static class RoleExt
    {
        public static bool IsWebPermitted(this Role role)
        {
            return role == Role.Developer || role == Role.Root || 
                   role == Role.WebOperator || role == Role.WebSupervisor;
        }

        public static bool IsSuperclientPermitted(this Role role)
        {
            return role == Role.Developer || role == Role.Superclient;
        }

        public static bool IsDesktopPermitted(this Role role)
        {
            return role == Role.Developer || role == Role.Root || 
                   role == Role.Operator || role == Role.Supervisor;
        }
    }
}