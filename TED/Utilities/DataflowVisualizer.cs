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

            GraphViz<TablePredicate>.Cluster? baseTables = null; // g.MakeCluster("base tables");
            //baseTables.Attributes["label"] = "Base Tables";

            var baseTableClusters = new Dictionary<TablePredicate, GraphViz<TablePredicate>.Cluster>();

            GraphViz<TablePredicate>.Cluster ClusterOfBaseTable(TablePredicate p2)
            {
                if (baseTableClusters.TryGetValue(p2, out var c))
                    return c;
                c = g.MakeCluster(p2.Name, baseTables);
                baseTableClusters[p2] = c;
                return c;
            }

            foreach (var t in p.Tables)
                if (t.Property.TryGetValue(TablePredicate.UpdaterFor, out var baseTable))
                    ClusterOfBaseTable((TablePredicate)baseTable);

            g.NodeCluster =
                t => t.IsExtensional
                    ? (baseTableClusters.ContainsKey(t) ? ClusterOfBaseTable(t) : baseTables)
                    : t.Property.TryGetValue(TablePredicate.UpdaterFor, out var baseTable)
                        ? (ClusterOfBaseTable((TablePredicate)baseTable))
                        : null;

            g.AddReachableFrom(p.Tables, Dependencies);
            return g;
        }

        private static string PredicateLabel(TablePredicate p)
        {
            if (p.Property.TryGetValue(TablePredicate.VisualizerName, out var s))
                return (string)s;
            return p.Name;
        }

        private static readonly Dictionary<string, object> InitialValueEdgeAttributes = new Dictionary<string, object>()
        {
            { "color", "blue" }
        };

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
            var operatorDependencies = p.OperatorDependencies ?? Array.Empty<TablePredicate>();
            var dependencies = (p.Rules == null)
                ? operatorDependencies
                : p.Rules.SelectMany(r => r.Dependencies);
            var dependencyEdges = dependencies.Select(dep => new GraphViz<TablePredicate>.Edge(dep, p));
            var inputs = p.Inputs.Select(i => new GraphViz<TablePredicate>.Edge(i, p, true, null, InputEdgeAttributes));
            if (p.InitialValueTable != null)
            {
                inputs = inputs.Append(
                    new GraphViz<TablePredicate>.Edge(p.InitialValueTable, p, true, null, InitialValueEdgeAttributes));
            }
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
