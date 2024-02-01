using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TED.Utilities {
    using static Enum;
    using static TextReader;

    /// <summary>
    /// Extremely simplistic CSV reader.
    /// Doesn't handle embedded commas, quotes, or newlines in cells.
    /// </summary>
    public static class CsvReader {
        #region File reading
        /// <summary> TextReader that stores the text file when opened </summary>
        private static TextReader _input = Null;
        /// <summary> Buffer of characters read in that are used to build a string </summary>
        private static readonly StringBuilder CellBuffer = new StringBuilder();
        /// <summary> List of rows as strings </summary>
        private static readonly List<string> RowBuffer = new List<string>();

        /// <summary> Return the next character, without advancing </summary>
        private static char Peek => (char)_input.Peek();

        /// <summary> Return the next character and advance one character </summary>
        private static char Read => (char)_input.Read();

        /// <summary> Advance without returning the next character </summary>
        private static void Skip() => _input.Read();

        /// <summary> True if we're at the end of the stream </summary>
        private static bool End => _input.Peek() < 0;

        /// <summary> True if the next character is a newline </summary>
        private static bool EndOfLineChar {
            get {
                var next = Peek;
                return next == '\n' || next == '\r';
            }
        }

        /// <summary> Moves the cell string buffer into the row buffer </summary>
        private static void AddCellToRow() {
            RowBuffer.Add(CellBuffer.ToString());
            CellBuffer.Clear();
        }

        /// <summary>  Read a quoted column into the cell string buffer. </summary>
        private static void ReadQuotedCell() {
            while (!EndOfLineChar && !End) {
                var ch = Read;
                if (ch != '"') CellBuffer.Append(ch);
                else {                      // Quote marks the end of the column
                    if (Peek != '"') break; // Check if it's an escaped quote
                    CellBuffer.Append('"');
                    Skip(); // Skip second quote mark
                }
            }
        }

        /// <summary> Read a row from the spreadsheet. </summary>
        /// <returns>Array of column strings</returns>
        private static string[] GetRow() {
            CellBuffer.Clear();
            RowBuffer.Clear();
            while (!EndOfLineChar && !End) {
                var ch = Read;
                switch (ch) {
                    // End of the row, falls through while after this
                    case '\n':
                    case '\r':
                        break;
                    // This case handles adding built cells to the row buffer (for all but the last column)
                    case ',':
                    case '\t':
                        AddCellToRow();
                        break;
                    // The following cases build a string from the contents of a cell
                    case '"':
                        ReadQuotedCell();
                        break;
                    default:
                        CellBuffer.Append(ch);
                        break;
                }
            }
            // Now at EOL, skip until past newlines
            while (EndOfLineChar) Skip();
            // Add the remaining strings to the buffer (final column not added in while)
            AddCellToRow();
            return RowBuffer.ToArray();
        }
        #endregion

        /// <summary> Read a CSV file. </summary>
        /// <param name="path">Path to the csv file to be read</param>
        /// <returns>Tuple of the header line as an array of strings and the rest as an array of arrays of strings</returns>
        public static (string[] Header, string[][] data) ReadCsv(string path) {
            _input = File.OpenText(path);
            var rows = new List<string[]>();
            while (!End) rows.Add(GetRow());
            return (rows[0], rows.Skip(1).ToArray());
        }

        #region Parsing
        /// <summary> Mapping of types to functions that convert a string to the specified type. </summary>
        private static readonly Dictionary<Type, Func<string, object>> ParserTable =
            new Dictionary<Type, Func<string, object>>();

        private static object ConvertCellInternal(string cell, Type t) =>
            ParserTable.TryGetValue(t, out var parser) ? parser(cell) :
            t == typeof(string) ? cell :
            t == typeof(int) ? int.Parse(cell) :
            t == typeof(uint) ? uint.Parse(cell) :
            t == typeof(double) ? double.Parse(cell) :
            t == typeof(float) ? float.Parse(cell) :
            t == typeof(bool) ? bool.Parse(cell) :
            t.IsEnum ? Parse(t, cell, true) :
            throw new ArgumentException($"Can't convert cells of type {t.Name}");
        #endregion

        /// <summary>
        /// Assigns a custom method for converting the string in a cell to a particular data type.
        /// </summary>
        /// <param name="t">Type to be parsed</param>
        /// <param name="parser">Function that will return the object given the string</param>
        public static void DeclareParser(Type t, Func<string, object> parser) => ParserTable[t] = parser;
        
        /// <summary> Convert a string representation of a value to a specific type. </summary>
        /// <typeparam name="T">Type to convert the string to</typeparam>
        /// <param name="cell">String to be converted to type T</param>
        /// <returns>Object of type T with value converted from the cell string</returns>
        public static T ConvertCell<T>(string cell) => (T)ConvertCellInternal(cell, typeof(T));
    }
}
