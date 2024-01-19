namespace Iit.Fibertest.RtuMngr
{
    public class DtoInDbEf
    {
        public int Id { get; init; }
        public DateTime Registered { get; set; }

        public string Json { get; set; } = null!;
    }
}
