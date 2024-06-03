using System;
using System.Collections.Generic;
using System.Linq;

namespace TED.Utilities
{
    /// <summary>
    /// Makes GraphViz visualizations of the data flow from one TED TablePredicate to another
    /// </summary>
    public static class UpdateFlowVisualizer
    {
        /// <summary>
        /// Make a visualization of the dataflow between predicates in a Program or Simulation.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="path"></param>
        public static void MakeGraph(Program p, string path) => MakeGraph(p).WriteGraph(path);

        public static GraphViz<TablePredicate> MakeGraph(Program p)
        {
            var g = new GraphViz<TablePredicate>
            {
                NodeLabel = PredicateLabel,
                DefaultNodeAttributes = TableAttributes,
                DefaultEdgeAttributes = EdgeAttributes,
                GlobalNodeAttributes =
                {
                    ["style"] = "filled"
                }
            };

            g.AddReachableFrom(p.Tables.Where(t => t.IsDynamic), Dependencies);
            return g;
        }

        private static string PredicateLabel(TablePredicate p)
        {
            if (p.Property.TryGetValue(TablePredicate.VisualizerName, out var s))
                return (string)s;
            return p.Name;
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
            var dependencyEdges = p.UpdatePrerequisites.Select(dep => new GraphViz<TablePredicate>.Edge(dep, p));
            var inputs = p.Inputs.Select(i => new GraphViz<TablePredicate>.Edge(i, p, true, null, InputEdgeAttributes));
            var setters = p.ColumnUpdateTables.Select(s => new GraphViz<TablePredicate>.Edge(s, p, true, null, SetEdgeAttributes)).ToArray();
            return dependencyEdges.Concat(inputs).Concat(setters);
        }

        private static IEnumerable<KeyValuePair<string, object>> TableAttributes(TablePredicate p)
        {
            var attributes = new Dictionary<string, object>();
            
            if (p.IsStatic) {
                // static EBD == true static table (equivalent to Datalog EBD)
                if (p.IsExtensional) attributes["fillcolor"] = "gold";
                // static IBD == initially tables (and other static tables that are constructed via rules)
                if (p.IsIntensional) attributes["fillcolor"] = "deepskyblue";
                attributes["shape"] = "diamond";
            } else if (p.IsExtensional)
                // dynamic EBD == base tables (our custom EBD tables - allows init, accum, & set)
                attributes["fillcolor"] = "limegreen";
            // dynamic IDB == true dynamic table (equivalent to Datalog IBD),
            //      these tables are fully recomputed each tick

            if (p.UpdatePrerequisites.Count == 0)
                attributes["rank"] = "min";
            return attributes;
        }

        private static Dictionary<string, object> EdgeAttributes(GraphViz<TablePredicate>.Edge e)
        {
            var attributes = new Dictionary<string, object>();
            return attributes;
        }
    }
}
