using System;

namespace Iit.Fibertest.TestBench
{
    public class Licence
    {
        public Guid Id { get; set; }
        public int AllowedRtuCount { get; set; }
        public int AllowedClientCount { get; set; }
    }
}