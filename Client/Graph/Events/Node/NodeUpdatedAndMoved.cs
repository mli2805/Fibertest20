using System;
using GMap.NET;

[Serializable]
public class NodeUpdatedAndMoved
{
    public Guid NodeId { get; set; }
    public string Title { get; set; }
    public PointLatLng GpsCoors { get; set; }
    public string Comment { get; set; }

}