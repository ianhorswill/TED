using System;
using System.Collections.Generic;
using System.Linq;

namespace TED.Utilities
{
    /// <summary>
    /// Makes GraphViz visualizations of the data flow from one TED TablePredicate to another
    /// </summary>
    public static class DataflowVisualizer
    {
        /// <summary>
        /// Make a visualization of the dataflow between predicates in a Program or Simulation.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="path"></param>
        public static void MakeGraph(Program p, string path)
        {

            var g = new GraphViz<TablePredicate>
            {
                NodeLabel = t => t.Name,
                DefaultNodeAttributes = TableAttributes,
                GlobalNodeAttributes =
                {
                    ["style"] = "filled"
                }
            };

            var baseTables = g.MakeCluster("base tables");
            //baseTables.Attributes["label"] = "Base Tables";
            g.NodeCluster = t => t.IsExtensional ? baseTables : null;

            g.AddReachableFrom(p.Tables, Dependencies);
            g.WriteGraph(path);
        }

        private static readonly Dictionary<string, object> InputEdgeAttributes = new Dictionary<string, object>()
        {
            { "color", "green" }
        };

        private static readonly Dictionary<string, object> SetEdgeAttributes = new Dictionary<string, object>()
        {
            { "color", "red" }
        };

        private static IEnumerable<GraphViz<TablePredicate>.Edge> Dependencies(TablePredicate p)
        {
            var dependencies = (p.Rules == null)
                ? Array.Empty<GraphViz<TablePredicate>.Edge>()
                : p.Rules
                    .SelectMany(r => r.Dependencies)
                    .Select(dep => new GraphViz<TablePredicate>.Edge(dep, p));
            var inputs = p.Inputs.Select(i => new GraphViz<TablePredicate>.Edge(i, p, true, null, InputEdgeAttributes)).ToArray();
            var setters = p.UpdateTables.Select(s => new GraphViz<TablePredicate>.Edge(s, p, true, null, SetEdgeAttributes)).ToArray();
            return dependencies.Concat(inputs).Concat(setters);
        }

        private static IEnumerable<KeyValuePair<string, object>> TableAttributes(TablePredicate p)
        {
            if (p.IsIntensional)
                return Array.Empty<KeyValuePair<string, object>>();
            return new[]
            {
                new KeyValuePair<string, object>("style", "filled"),
                new KeyValuePair<string, object>("fillcolor", "greenyellow")
            };
        }
    }
}
