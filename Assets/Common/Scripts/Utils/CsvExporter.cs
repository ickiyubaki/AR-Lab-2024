using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Common.Scripts.Utils
{
    public class CsvExporter<T>
    {
        public string Write(IEnumerable<T> objects, string fileName)
        {
            var filePath = Path.Combine(Application.temporaryCachePath, fileName);
            var objs = objects as IList<T> ?? objects.ToList();

            if (objs.Any())
            {
                using (var sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    sw.WriteLine(ToCsv(objs[0], true));

                    foreach (var obj in objs)
                    {
                        sw.WriteLine(ToCsv(obj));
                    }
                }
            }

            return filePath;
        }

        public string Write(Dictionary<string, string> data, string fileName)
        {
            var filePath = Path.Combine(Application.temporaryCachePath, fileName);

            if (data.Any())
            {
                using (var sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    sw.WriteLine(ToCsv(data.Keys.ToList()));
                    sw.WriteLine(ToCsv(data.Values.ToList()));
                }
            }

            return filePath;
        }

        private string ToCsv(T obj, bool header = false)
        {
            var output = "";

            IReadOnlyList<PropertyInfo> properties = obj.GetType().GetProperties();

            for (var i = 0; i < properties.Count; i++)
            {
                output += header ? PreProcess(properties[i].Name) : PreProcess(properties[i].GetValue(obj)?.ToString() ?? "");

                if (i != properties.Count - 1)
                {
                    output += ",";
                }
            }

            return output;
        }

        private string ToCsv(IReadOnlyList<string> data)
        {
            var output = "";

            for (var i = 0; i < data.Count; i++)
            {
                output += PreProcess(data[i]);

                if (i != data.Count - 1)
                {
                    output += ",";
                }
            }

            return output;
        }

        private string PreProcess(string value)
        {
            var output = value?.Trim() ?? "";
            if (output.Contains(",") || output.Contains("\"") || output.Contains("\n") || output.Contains("\r"))
                output = '"' + output.Replace("\"", "\"\"") + '"';

            return output;
        }
    }
}