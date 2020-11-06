using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ResTB.GUI.Helpers
{
    public class SerializationHelper
    {

        /// <summary>
        /// Generate a valid filename without special characters and without spaces and brackets, no extension supported
        /// </summary>
        /// <param name="input">input string with special characters</param>
        /// <returns>string without special characters and without spaces and brackets</returns>
        public static string makeValidFileName(string input)
        {
            string output = input.Trim();

            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                output = output.Replace(c, '_');
            }

            output = output.Replace(' ', '_');
            output = output.Replace('(', '_');
            output = output.Replace(')', '_');
            output = output.Replace('.', '_');
            output = output.Replace(',', '_');

            return output;
        }

        /// <summary>
        /// Using XML serialization to deep copy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeepCopyXML<T>(T obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlSerializer ser = new XmlSerializer(typeof(T));
                ser.Serialize(stream, obj);
                stream.Position = 0;

                return (T)ser.Deserialize(stream);
            }
        }
    }
}
