using System;

namespace Iit.Fibertest.Graph.Commands
{
    public class UpdateNode
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Comment { get; set; }
    }
}