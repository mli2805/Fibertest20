using System;

namespace Iit.Fibertest.Dto
{
    [Serializable]
    public enum EquipmentType
    {
        Error = -1,


        AdjustmentPoint = 100,

        EmptyNode = 200,

        CableReserve = 300,

        Other = 400,
        Closure = 402, 
        Cross = 403,
        Well = 405,
        Terminal = 406,

        Rtu = 500,
        AccidentPlace = 501,
    }
}
