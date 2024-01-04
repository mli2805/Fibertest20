namespace Iit.Fibertest.RtuDaemon;

public class LongOperation
{
    public Guid CommandGuid { get; set; }
    public DateTime QueuingTime { get; set; }

    public string Json { get; set; }
    public bool IsProcessed { get; set; }

    public string? ResultJson { get; set; }

    public LongOperation(string json)
    {
        CommandGuid = Guid.NewGuid();
        QueuingTime = DateTime.Now;
        Json = json;
    }
}