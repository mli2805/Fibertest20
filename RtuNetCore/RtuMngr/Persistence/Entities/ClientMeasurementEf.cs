using Iit.Fibertest.Dto;

namespace Iit.Fibertest.RtuMngr;

public class ClientMeasurementEf
{
    public int Id { get; set; }
    public Guid ClientMeasurementId { get; set; }
    public string ConnectionId { get; set; } = null!;
    public ReturnCode ReturnCode { get; set; }
    public byte[]? SorBytes { get; set; }

    public string Serial { get; set; } = null!;
    public bool IsPortOnMainCharon { get; set; }
    public int OpticalPort { get; set; }
    public int MainCharonPort { get; set; }
}