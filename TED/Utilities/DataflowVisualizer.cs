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
                NodeLabel = PredicateLabel,
                DefaultNodeAttributes = TableAttributes,
                DefaultEdgeAttributes = EdgeAttributes,
                GlobalNodeAttributes =
                {
                    ["style"] = "filled"
                }
            };


            GraphViz<TablePredicate>.Cluster? baseTables = null; // g.MakeCluster("base tables");
            //baseTables.Attributes["label"] = "Base Tables";

            var baseTableClusters = new Dictionary<TablePredicate, GraphViz<TablePredicate>.Cluster>();

            GraphViz<TablePredicate>.Cluster ClusterOfBaseTable(TablePredicate p)
            {
                if (baseTableClusters.TryGetValue(p, out var c))
                    return c;
                c = g.MakeCluster(p.Name, baseTables);
                baseTableClusters[p] = c;
                return c;
            }

            foreach (var t in p.Tables)
                if (t.Property.TryGetValue(TablePredicate.UpdaterFor, out var baseTable))
                    ClusterOfBaseTable((TablePredicate)baseTable);

            g.NodeCluster = 
                t => t.IsExtensional ? 
                    (baseTableClusters.ContainsKey(t)?ClusterOfBaseTable(t):baseTables) 
                    : t.Property.TryGetValue(TablePredicate.UpdaterFor, out var baseTable)?
                        (ClusterOfBaseTable((TablePredicate)baseTable))
                        :null;

            g.AddReachableFrom(p.Tables, Dependencies);
            g.WriteGraph(path);
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
            var dependencies = (p.Rules == null)
                ? Array.Empty<GraphViz<TablePredicate>.Edge>()
                : p.Rules
                    .SelectMany(r => r.Dependencies)
                    .Select(dep => new GraphViz<TablePredicate>.Edge(dep, p));
            var inputs = p.Inputs.Select(i => new GraphViz<TablePredicate>.Edge(i, p, true, null, InputEdgeAttributes)).ToArray();
            var setters = p.ColumnUpdateTables.Select(s => new GraphViz<TablePredicate>.Edge(s, p, true, null, SetEdgeAttributes)).ToArray();
            return dependencies.Concat(inputs).Concat(setters);
        }

        private static IEnumerable<KeyValuePair<string, object>> TableAttributes(TablePredicate p)
        {
            var attributes = new Dictionary<string, object>();
            if (p.IsExtensional)
                attributes["fillcolor"] = "greenyellow";

            return attributes;
        }

        private static Dictionary<string, object> EdgeAttributes(GraphViz<TablePredicate>.Edge e)
        {
            var attributes = new Dictionary<string, object>();
            if (e.EndNode.Property.ContainsKey(TablePredicate.UpdaterFor))
                attributes["constraint"] = "false";
            return attributes;
        }
    }
}
