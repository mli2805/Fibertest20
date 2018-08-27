using System.Collections.Generic;

namespace KadastrLoader
{
    public class LoadedAlready
    {
        public List<Well> Wells { get; set; } = new List<Well>();
        public List<Conpoint> Conpoints { get; set; } = new List<Conpoint>();
    }
}