using System.IO;

namespace Iit.Fibertest.TestBench
{
    public class Export
    {
        private readonly ReadModel _readModel;

        public Export(ReadModel readModel)
        {
            _readModel = readModel;
        }

        public void F()
        {
            string[] lines = File.ReadAllLines("export.txt");

            foreach (var line in lines)
            {
                var parts = line.Split('|')[1].Split(';');
            }
        }
    }
}
