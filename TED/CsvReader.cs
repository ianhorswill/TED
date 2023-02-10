
using System;
using System.IO;
using System.Linq;

namespace TED
{
    /// <summary>
    /// Extremely simplistic CSV reader.  Doesn't handle embedded commas, quotes, or newlines in cells
    /// </summary>
    public static class CsvReader
    {
        /// <summary>
        /// Convert a sting representation of a value to a specific type
        /// </summary>
        public static object ConvertCellInternal(string cell, Type t)
        {
            if (t == typeof(string))
                return cell;
            if (t == typeof(int))
                return int.Parse(cell);
            if (t == typeof(float))
                return float.Parse(cell);
            if (t.IsEnum)
                return Enum.Parse(t, cell, true);
            throw new ArgumentException($"Can't convert cells of type {t.Name}");
        }

        /// <summary>
        /// Convert a string representation of a value to a specific type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static T ConvertCell<T>(string cell) => (T)ConvertCellInternal(cell, typeof(T));

        /// <summary>
        /// Read a CSV file.
        /// Returns the header line as an array of strings and the rest as an array of arrays of strings
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static (string[] Header, string[][] data) ReadCsv(string path)
        {
            var data = File.ReadAllLines(path).Select(line => line.Split(',')).ToArray();
            return (data[0], data.Skip(1).ToArray());
        }
    }
}
