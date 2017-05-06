using System;

namespace Iit.Fibertest.Client
{
    public class Licence
    {
        public Guid Id { get; set; }
        public int AllowedRtuCount { get; set; }
        public int AllowedClientCount { get; set; }
    }
}