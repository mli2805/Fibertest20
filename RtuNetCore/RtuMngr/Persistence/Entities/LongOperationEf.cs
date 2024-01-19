namespace Iit.Fibertest.RtuMngr;

public class LongOperationEf
{
    public int Id { get; init; }
    public Guid CommandGuid { get; set; }
    public DateTime QueuingTime { get; set; }

    public string Json { get; set; } // operation request dto
    public bool IsReady { get; set; }

    public string? ResultJson { get; set; }

    public LongOperationEf(string json)
    {
        CommandGuid = Guid.NewGuid();
        QueuingTime = DateTime.Now;
        Json = json;
    }
}