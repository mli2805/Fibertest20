namespace Iit.Fibertest.Client
{
    public class GraphCount
    {
        public int Rtus;
        public int Eqs;
        public int Wells;
        public int Total;

        public override string ToString()
        {
            return $@"{Rtus} / {Eqs} / {Wells} / {Total}";
        }
    }
}