namespace Iit.Fibertest.Client
{
    public class OtdrParametersTemplate
    {
        public string Title { get; set; }
        public string Lmax;
        public string Dl;
        public string Tp;
        public string Time;
        public string Description => $@"Lmax = {Lmax} km;   dL = {Dl} m;   Tp = {Tp} ns;   t = {Time}";

        public bool IsChecked { get; set; }
    }
}