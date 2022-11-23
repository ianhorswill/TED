
using System;
using System.Collections;
using System.IO;
using System.Linq;

namespace TED
{
    public static class CsvReader
    {
        public static object ConvertCellInternal(string cell, Type t)
        {
            if (t == typeof(string))
                return cell;
            if (t == typeof(int))
                return int.Parse(cell);
            if (t == typeof(float))
                return float.Parse(cell);
            throw new ArgumentException($"Can't convert cells of type {t.Name}");
        }

        public static T ConvertCell<T>(string cell) => (T)ConvertCellInternal(cell, typeof(T));

        public static (string[] Header, string[][] data) ReadCsv(string path)
        {
            var data = File.ReadAllLines(path).Select(line => line.Split(',')).ToArray();
            return (data[0], data.Skip(1).ToArray());
        }
    }
}
