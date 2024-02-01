using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

namespace TED.Utilities {
    using static String;
    using static File;

    /// <summary>
    /// Handler for custom ToStrings that are used in the creation of CSVs
    /// </summary>
    public static class CsvWriter {
        /// <summary> Mapping of types to functions that convert the specified type to a string. </summary>
        private static readonly Dictionary<Type, Func<object, string>> WriterTable =
            new Dictionary<Type, Func<object, string>>();
        /// <summary> String builder that creates the entire CSV in one WriteAllText </summary>
        private static readonly StringBuilder CsvBuilder = new StringBuilder();
        /// <summary> Buffer of strings in an array converted from the content of one row in a table </summary>
        private static readonly string[] RowBuffer = new string[8]; // no more than 8 columns ever...

        /// <summary> Assigns a custom method for converting a particular data type into a string. </summary>
        /// <param name="t">Type to be written</param>
        /// <param name="writer">Function that will return a string given the object</param>
        public static void DeclareWriter(Type t, Func<object, string> writer) => WriterTable[t] = writer;

        /// <summary>Whether or not the given type has a writer func in the WriterTable</summary>
        /// <param name="t">Type to check in the WriterTable</param>
        /// <returns>True if the type has a writer function in the WriterTable</returns>
        public static bool HasDeclaredWriter(Type t) => WriterTable.ContainsKey(t);
        /// <summary>The writer func from WriterTable for a given type</summary>
        /// <param name="t">Type to get a function for from WriterTable</param>
        /// <returns>The function that maps objects of the given type to a string</returns>
        public static Func<object, string> GetDeclaredWriter(Type t) => WriterTable[t];

        /// <summary> Null-tolerant, tries to get an appropriate CsvWriter falling back on ToString. </summary>
        internal static string Stringify<T>(in T value) => value == null ? "null" :
            WriterTable.TryGetValue(value.GetType(), out var writer) ?
                writer(value) : value.ToString();
        
        /// <summary> Writes an entire table out to a CSV file </summary>
        /// <param name="path">Path to where the CSV file will be written</param>
        /// <param name="table">Table to write out as a CSV file</param>
        public static void TableToCsv(string path, TablePredicate table) {
            CsvBuilder.AppendLine(Join(", ", table.ColumnHeadings));
            for (uint i = 0; i < table.Length; i++) {
                table.RowToCsv(i, RowBuffer);
                CsvBuilder.AppendLine(Join(", ", RowBuffer, 0 , table.ColumnHeadings.Length));
            }
            WriteAllText($"{path}/{table.Name}.csv", CsvBuilder.ToString());
            CsvBuilder.Clear();
        }

        /// <summary> Writes every table in a Simulation out to individual CSV files </summary>
        /// <param name="path">Path to where the CSV files will be written</param>
        /// <param name="simulation">Simulation that's tables will be written out as CSV files</param>
        public static void WriteAllTables(string path, Simulation simulation) {
            foreach (var table in simulation.Tables) TableToCsv(path, table);
        }
    }
}
