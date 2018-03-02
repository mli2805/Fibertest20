namespace Iit.Fibertest.Graph
{
    public enum OpticalAccidentType
    {
        Break,                   // B,  обрыв
        Loss,                    // L,  превышение порога затухания
        Reflectance,             // R,  превышение порога коэффициента отражения 
        LossCoeff,               // C,  превышение порога коэффициента затухания

        None,


    }

    public static class OpticalAccidentTypeExt
    {
        public static string ToLetter(this OpticalAccidentType type)
        {
            switch (type)
            {
                case OpticalAccidentType.Break: return @"B";
                case OpticalAccidentType.Reflectance: return @"R";
                case OpticalAccidentType.Loss: return @"L";
                case OpticalAccidentType.LossCoeff: return @"C";
                default: return @"N";
            }
        }
    }
}