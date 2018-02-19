using System;

namespace Iit.Fibertest.Dto
{
    public class Zone
    {
        public int Id { get; set; }
        public Guid ZoneId { get; set; }

        public string Title { get; set; }
        public string Comment { get; set; }

    }
}