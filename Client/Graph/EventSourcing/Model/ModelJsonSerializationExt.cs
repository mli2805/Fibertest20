using System.IO;
using Newtonsoft.Json;

namespace Iit.Fibertest.Graph
{
    public static class ModelJsonSerialization
    {
        /// <summary>
        /// Writes the given object instance to a Json file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// <para>Only Public properties and variables will be written to the file.
        /// These can be any type though, even other classes.</para>
        /// <para>If there are public properties/variables that you do not want written to the file,
        /// decorate them with the [JsonIgnore] attribute.</para>
        /// </summary>
        /// <param name="model"></param>
        /// <param name="filePath">The file path to write the object instance to.</param>
        public static void WriteToJsonFile(this Model model, string filePath)
        {
            TextWriter writer = null;
            try
            {
                var contentsToWriteToFile = JsonConvert.SerializeObject(model);
                writer = new StreamWriter(filePath);
                writer.Write(contentsToWriteToFile);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

    }
}
