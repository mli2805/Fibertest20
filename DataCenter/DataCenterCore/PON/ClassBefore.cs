namespace Iit.Fibertest.DataCenterCore
{
    public class ClassBefore
    {
        public int  Prop { get; set; }
        public ClassBefore()
        {
            Prop = 0;
        }

        public void Add(int p)
        {
            Prop += p;
        }
    }
}
