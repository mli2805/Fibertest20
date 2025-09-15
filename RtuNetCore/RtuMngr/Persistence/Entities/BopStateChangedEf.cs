using System.ComponentModel.DataAnnotations;

namespace Iit.Fibertest.RtuMngr;

public class BopStateChangedEf
{
    public int Id { get; set; }
    public Guid RtuId { get; set; }

    [MaxLength(32)]
    public string Serial { get; set; } = null!;
    [MaxLength(32)]
    public string OtauIp { get; set; } = null!;
    public int TcpPort { get; set; }
    public bool IsOk { get; set; }
}