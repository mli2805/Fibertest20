using System;

namespace Iit.Fibertest.Dto
{
    public class Snapshot
    {
        public int Id { get; set; }
        public Guid StreamIdOriginal { get; set; }
        public int LastEventNumber { get; set; }
        public DateTime LastEventDate { get; set; }
        public byte[] Payload { get; set; }
    } 
    
    // не менять этот класс - иначе вручную править создание таблицы в Snapshot30TableCreator
    public class Snapshot30
    {
        public int Id { get; set; }
        public Guid StreamIdOriginal { get; set; }
        public int LastEventNumber { get; set; }
        public DateTime LastEventDate { get; set; }
        public byte[] Payload { get; set; }

        public long PayloadLength { get; set; }
    }
}