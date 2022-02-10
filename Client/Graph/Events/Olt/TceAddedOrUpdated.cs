using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class TceAddedOrUpdated
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public TceType TceType { get; set; }
        public string Ip { get; set; }
        public string Comment { get; set; }
    }
}
