namespace Iit.Fibertest.Dto
{
    public enum OpticalAccidentType
    {
        Break,                   // B,  обрыв
        Loss,                    // L,  превышение порога затухания
        Reflectance,             // R,  превышение порога коэффициента отражения 
        LossCoeff,               // C,  превышение порога коэффициента затухания
        TotalLoss,               // ?,  превышение полного затухания в линии

        None,
    }
}