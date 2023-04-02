
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TED.Utilities
{
    /// <summary>
    /// Extremely simplistic CSV reader.  Doesn't handle embedded commas, quotes, or newlines in cells
    /// </summary>
    public class CsvReader
    {
        /// <summary>
        /// Read a CSV file.
        /// Returns the header line as an array of strings and the rest as an array of arrays of strings
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static (string[] Header, string[][] data) ReadCsv(string path)
        {
            var data = new CsvReader(path).GetRows();
            return (data[0], data.Skip(1).ToArray());
        }


        #region Cell parsing
        /// <summary>
        /// Convert a string representation of a value to a specific type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static T ConvertCell<T>(string cell) => (T)ConvertCellInternal(cell, typeof(T));

        /// <summary>
        /// Convert a sting representation of a value to a specific type
        /// </summary>
        public static object ConvertCellInternal(string cell, Type t)
        {
            if (ParserTable.TryGetValue(t, out var parser))
                return parser(cell);
            if (t == typeof(string))
                return cell;
            if (t == typeof(int))
                return int.Parse(cell);
            if (t == typeof(uint))
                return uint.Parse(cell);
            if (t == typeof(double))
                return double.Parse(cell);
            if (t == typeof(float))
                return float.Parse(cell);
            if (t == typeof(bool))
                return bool.Parse(cell);
            if (t.IsEnum)
                return Enum.Parse(t, cell, true);
            throw new ArgumentException($"Can't convert cells of type {t.Name}");
        }

        /// <summary>
        /// Provide a custom method for converting the string in a cell to a particular data type
        /// </summary>
        /// <param name="t">Type to be parsed</param>
        /// <param name="parser">Function that will return the object given the string</param>
        public static void DeclareParser(Type t, Func<string, object> parser) => ParserTable[t] = parser;

        private static readonly Dictionary<Type, Func<string, object>> ParserTable =
            new Dictionary<Type, Func<string, object>>();
        #endregion

        #region Parsing
        ///// <summary>
        ///// Path to file being read from, if any
        ///// </summary>
        //private readonly string? FilePath;

        ///// <summary>
        ///// Line number of file being read from
        ///// </summary>
        //private int LineNumber { get; private set; }

        private readonly TextReader input;


        /// <summary>
        /// True if we're at the end of the stream
        /// </summary>
        private bool End => input.Peek() < 0;

        ///// <summary>
        ///// Return the current character, without advancing
        ///// </summary>
        //private char Peek => (char)(input.Peek());

        private readonly StringBuilder stringBuffer = new StringBuilder();
        private readonly List<string> rowBuffer = new List<string>();

        private CsvReader(string filePath) : this(File.OpenText(filePath))
        { }

        private CsvReader(TextReader input)
        {
            this.input = input;
        }

        bool EndOfLineChar
        {
            get
            {
                var next = input.Peek();
                return next == '\n' || next == '\r';
            }
        }

        private List<string[]> GetRows()
        {
            var rows = new List<string[]>();
            while (!End)
                rows.Add(GetRow());
            return rows;
        }

        /// <summary>
        /// Read a row from the spreadsheet.
        /// </summary>
        /// <returns>Array of column strings</returns>
        private string[] GetRow()
        {
            void FinishColumn()
            {
                rowBuffer.Add(stringBuffer.ToString());
                stringBuffer.Clear();
            }

            stringBuffer.Clear();
            rowBuffer.Clear();

            while (!EndOfLineChar && !End)
            {
                char ch;
                switch (ch = (char)input.Read())
                {
                    case '\n':
                    case '\r':
                        break;

                    case ',':
                    case '\t':
                        FinishColumn();
                        break;

                    case '"':
                        ReadQuoted();
                        break;

                    default:
                        stringBuffer.Append(ch);
                        break;
                }
            }

            // Now at EOL.
            while (EndOfLineChar) input.Read();

            rowBuffer.Add(stringBuffer.ToString());
            stringBuffer.Clear();

            return rowBuffer.ToArray();

            // ReSharper disable once PossibleNullReferenceException
            //var line = Input.ReadLine();
            //Debug.Assert(line != null, nameof(line) + " != null");
            //if (LineNumber == 1 && line.Contains('\t'))
            //    separator = '\t';
            //return line.Split(separator).Select(s => s.Trim(' ', '"')).ToArray();
        }

        /// <summary>
        /// Read a quoted column
        /// This just reads into the string buffer.  We depend on the caller to move the string buffer into the row buffer.
        /// </summary>
        private void ReadQuoted()
        {
            while (!EndOfLineChar && !End)
            {
                char ch;
                if ((ch = (char)input.Read()) == '"')
                {
                    if (input.Peek() == '"')
                    {
                        // It's an escaped quote mark
                        stringBuffer.Append('"');
                        input.Read(); // Skip second quote mark
                    }
                    else
                        // Quote marks the end of the column
                        break;
                }
                else
                    stringBuffer.Append(ch);
            }
        }
        #endregion
    }
}
