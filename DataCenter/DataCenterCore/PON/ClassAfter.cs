namespace Iit.Fibertest.DataCenterCore
{
    public class ClassAfter
    {
        public int  Prop { get; set; }
        public ClassAfter()
        {
            Prop = 0;
        }

        public void Add(int p)
        {
            Prop += p;
        }
    }
}
