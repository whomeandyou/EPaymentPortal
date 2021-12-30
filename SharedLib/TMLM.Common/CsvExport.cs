using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;

namespace TMLM.Common {
    public class CsvExport<T> where T : class {
        public List<T> Objects;

        public CsvExport(List<T> objects) {
            Objects = objects;
        }

        public string Export() {
            return Export(null, true);
        }

        public string Export(List<PropertyInfo> excludeProp) {
            return Export(excludeProp, true);
        }

        public string Export(List<PropertyInfo> excludeProp, bool includeHeaderLine) {

            StringBuilder sb = new StringBuilder();
            //Get properties using reflection.
            IList<PropertyInfo> propertyInfos = typeof(T).GetProperties();

            if (includeHeaderLine) {
                //add header line.
                foreach (PropertyInfo propertyInfo in propertyInfos) {
                    if (excludeProp != null) {
                        if (excludeProp.Contains(propertyInfo))
                            continue;
                    }
                    sb.Append("\"" + propertyInfo.Name + "\"").Append(",");
                }
                sb.Remove(sb.Length - 1, 1).AppendLine();
            }

            //add value for each property.
            foreach (T obj in Objects) {
                foreach (PropertyInfo propertyInfo in propertyInfos) {
                    if (excludeProp != null) {
                        if (excludeProp.Contains(propertyInfo))
                            continue;
                    }
                    sb.Append(MakeValueCsvFriendly(propertyInfo.GetValue(obj, null))).Append(",");
                }
                sb.Remove(sb.Length - 1, 1).AppendLine();
            }

            return sb.ToString();
        }

        //export to a file.
        public void ExportToFile(string path) {
            File.WriteAllText(path, Export());
        }

        //export as binary data.
        public byte[] ExportToBytes() {
            return Encoding.UTF8.GetBytes(Export());
        }

        //export as binary data.
        public byte[] ExportToBytes(List<PropertyInfo> excludeProp) {
            return Encoding.UTF8.GetBytes(Export(excludeProp));
        }

        //get the csv value for field.
        private string MakeValueCsvFriendly(object value) {
            if (value == null) return "";
            if (value is Nullable && ((INullable)value).IsNull) return "";

            if (value is DateTime) {
                if (((DateTime)value).TimeOfDay.TotalSeconds == 0)
                    return ((DateTime)value).ToString("yyyy-MM-dd");
                return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
            }

            string output = value.ToString();

            if (output.Contains(",") || output.Contains("\""))
                output = '"' + output.Replace("\"", "\"\"") + '"';

            return output;

        }
    }
}
