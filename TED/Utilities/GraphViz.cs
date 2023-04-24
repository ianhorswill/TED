﻿using System;
using System.Collections.Generic;
using System.IO;

namespace TED.Utilities
{
    /// <summary>
    /// Represents a graph to be written to a .dot file for rendering using GraphViz
    /// This is the untyped base class.  Use the version with a type parameter to make a real graph.
    /// </summary>
    public abstract class GraphViz
    {
        internal static readonly Dictionary<string, object> EmptyAttributeDictionary = new Dictionary<string, object>();

        /// <summary>
        /// Attributes of the graph itself
        /// </summary>
        public readonly Dictionary<string, object> Attributes = new Dictionary<string, object>();
        /// <summary>
        /// Attributes to be applied by default to all nodes
        /// </summary>
        public readonly Dictionary<string, object> GlobalNodeAttributes = new Dictionary<string, object>();
        /// <summary>
        /// Attributes to be applied by default to all edges
        /// </summary>
        public readonly Dictionary<string, object> GlobalEdgeAttributes = new Dictionary<string, object>();

        internal int NextNodeUid;

        /// <summary>
        /// Write the value part of an attribute/value pair in .dot format
        /// </summary>
        internal static void WriteAttribute(object value, TextWriter o)
        {
            switch (value)
            {
                case string s:
                    WriteQuotedString(s, o);
                    break;

                default:
                    o.Write(value);
                    break;
            }
        }

        /// <summary>
        /// Write an attribute/value pair in .dot format
        /// </summary>
        /// <param name="attr">Name of the attribute</param>
        /// <param name="value">Value of the attribute</param>
        /// <param name="o">Stream to write to</param>
        internal static void WriteAttribute(string attr, object value, TextWriter o)
        {
            o.Write(attr);
            o.Write('=');
            WriteAttribute(value, o);
        }

        /// <summary>
        /// Write an attribute/value pair in .dot format
        /// </summary>
        /// <param name="attr">Name of the attribute + its value</param>
        /// <param name="o">Stream to write to</param>
        internal static void WriteAttribute(KeyValuePair<string, object> attr, TextWriter o)
            => WriteAttribute(attr.Key, attr.Value, o);

        /// <summary>
        /// Write a series of attribute/value pairs in .dot format
        /// </summary>
        /// <param name="attributes">List of attribute/value pairs</param>
        /// <param name="preamble">string to print before a pair</param>
        /// <param name="postamble">String to print after a pair</param>
        /// <param name="o">Stream to print to</param>
        internal static void WriteAttributeList(IEnumerable<KeyValuePair<string, object>> attributes,
            string? preamble, string? postamble,
            TextWriter o)
        {
            foreach (var p in attributes)
            {
                o.Write(preamble??"");
                WriteAttribute(p, o);
                o.Write(postamble??"");
            }
        }

        internal static void WriteQuotedString(string s, TextWriter o)
        {
            o.Write('"');
            foreach (var c in s)
            {
                if (c == '"' || c == '\\')
                    o.Write('\\');
                o.Write(c);
            }

            o.Write('"');
        }
    }

    /// <summary>
    /// A graph to be written to a .dot file for visualization using GraphViz
    /// </summary>
    /// <typeparam name="T">The data type of the nodes in the graph</typeparam>
    public class GraphViz<T> : GraphViz
    {
        /// <summary>
        /// Optional function to compute attributes of a node
        /// </summary>
        public Func<T, IEnumerable<KeyValuePair<string, object>>>? DefaultNodeAttributes;
        /// <summary>
        /// Optional function to compute attributes of an edge
        /// </summary>
        public Func<Edge, IDictionary<string, object>>? DefaultEdgeAttributes;

        /// <summary>
        /// Function to compute the ID string to use for a node in the file, as distinct from its label
        /// By default, this just assigns a serial number to each node.  But you can specify your own function.
        /// </summary>
        public Func<T, string> NodeId;

        /// <summary>
        /// Function to compute the label to display inside a node
        /// </summary>
        public Func<T, string> NodeLabel;

        /// <summary>
        /// Function to compute the cluster to assign a node to, if any.
        /// </summary>
        public Func<T, Cluster?>? NodeCluster; 
        
        /// <summary>
        /// The internal ID string assigned to a node
        /// This is not defined until the node is added to the graph
        /// </summary>
        public readonly Dictionary<T, string> IdOf = new Dictionary<T, string>();

        /// <summary>
        /// The set of all nodes in the graph
        /// </summary>
        private readonly HashSet<T> nodes = new HashSet<T>();

        /// <summary>
        /// The set of all nodes in the graph
        /// </summary>
        public ISet<T> Nodes => nodes;

        /// <summary>
        /// The attributes assigned to a given node
        /// </summary>
        public Dictionary<T, Dictionary<string, object>>
            NodeAttributes = new Dictionary<T, Dictionary<string, object>>();

        /// <summary>
        /// Make a graph to be rendered using GraphViz.
        /// </summary>
        public GraphViz()
        {
            NodeId = (_) => $"v{NextNodeUid++}";
            NodeLabel = v => v!.ToString();
            DefaultNodeAttributes = n => EmptyAttributeDictionary;
            DefaultEdgeAttributes = edge => EmptyAttributeDictionary;
        }

        /// <summary>
        /// Add a node to the graph.
        /// IF the node is already present in the graph, this does nothing.
        /// </summary>
        public void AddNode(T n)
        {
            if (nodes.Contains(n))
                return;
            nodes.Add(n);
            NodeAttributes[n] = new Dictionary<string, object>();
            if (DefaultNodeAttributes != null)
                foreach (var p in DefaultNodeAttributes(n))
                    NodeAttributes[n][p.Key] = p.Value;
            IdOf[n] = NodeId(n);
            var c = NodeCluster?.Invoke(n);
            if (c != null)
                c.Nodes.Add(n);
        }
        
        /// <summary>
        /// Add the edge.
        /// If edge is already present, merge the attributes of the edge with the attributes listed here
        /// </summary>
        public void AddEdge(Edge e)
        {
            if (edges.TryGetValue(e, out var canonical))
                canonical.AddAttributes(e);
            else
            {
                if (DefaultEdgeAttributes != null)
                    e.AddAttributes(DefaultEdgeAttributes(e));
                edges.Add(e);
                // This breaks AddReachableFrom
                //AddNode(e.InNode);
                //AddNode(e.OutNode);
            }
        }

        /// <summary>
        /// Add all the nodes listed, and all the nodes reachable from them via the nodeEdges.
        /// The edges are added too.
        /// In other words, this adds the connected components of all the nodes in roots.
        /// </summary>
        /// <param name="roots">All the nodes to start from</param>
        /// <param name="nodeEdges">Function to list the set of edges incident on a specified node</param>
        public void AddReachableFrom(IEnumerable<T> roots, Func<T, IEnumerable<Edge>> nodeEdges)
        {
            foreach (var root in roots)
                AddReachableFrom(root, nodeEdges);
        }

        /// <summary>
        /// Add this node and all the nodes reachable from it via nodeEdges.  The edges are added too.
        /// In other words, this adds the specified node's connected component.
        /// </summary>
        /// <param name="root">Node to start tracing from</param>
        /// <param name="nodeEdges">Function to list the set of edges incident on a specified node</param>
        public void AddReachableFrom(T root, Func<T, IEnumerable<Edge>> nodeEdges)
        {
            if (nodes.Contains(root))
                return;
            AddNode(root);
            foreach (var e in nodeEdges(root))
            {
                AddEdge(e);
                AddReachableFrom(e.StartNode,nodeEdges);
                AddReachableFrom(e.EndNode,nodeEdges);
            }
        }


        #region Output formatting
        /// <summary>
        /// Write the graph to the specified stream
        /// </summary>
        /// <param name="o">Stream to write to</param>
        public void WriteGraph(TextWriter o)
        {
            var writtenNodes = new HashSet<T>();
            void WriteCluster(Cluster c)
            {
                o.Write("subgraph ");
                WriteQuotedString("cluster_" + c.Name, o);
                o.WriteLine("{");

                WriteAttributeList(c.Attributes, "", "\n", o);

                foreach (var n in c.Nodes)
                    WriteNode(n, o);

                foreach (var s in c.SubClusters)
                    WriteCluster(s);

                o.WriteLine('}');
                writtenNodes.UnionWith(c.Nodes);
            }

            o.WriteLine("digraph {");

            WriteAttributeList(Attributes, "", "\n", o);

            if (GlobalNodeAttributes.Count > 0)
            {
                o.Write("node [");
                WriteAttributeList(GlobalNodeAttributes, " ", "", o);
                o.WriteLine("]");
            }

            if (GlobalEdgeAttributes.Count > 0)
            {
                o.Write("edge [");
                WriteAttributeList(GlobalEdgeAttributes, " ", "", o);
                o.WriteLine("]");
            }


            foreach (var c in topLevelClusters)
            {
                WriteCluster(c);
            }
            foreach (var v in nodes) 
                if (!writtenNodes.Contains(v))
                    WriteNode(v, o);
            foreach (var e in edges) WriteEdge(e, o);
            o.WriteLine("}");
        }

        /// <summary>
        /// Write the graph in .dot format to the specified file.
        /// </summary>
        /// <param name="path">Path to the file to write</param>
        public void WriteGraph(string path)
        {
            using var file = File.CreateText(path);
            WriteGraph(file);
        }

        /// <summary>
        /// Write an edge in DOT format
        /// </summary>
        private void WriteEdge(Edge edge, TextWriter o)
        {
            o.Write($"{IdOf[edge.StartNode]} -> {IdOf[edge.EndNode]}");
            if ((edge.Attributes != null && edge.Attributes.Count > 0) || !edge.Directed)
            {
                o.Write(" [");
                if (!edge.Directed)
                    o.Write("dir=none");
                if (edge.Attributes != null)
                    foreach (var pair in edge.Attributes)
                    {
                        o.Write(' ');
                        WriteAttribute(pair, o);
                    }
                o.Write(" ]");
            }
            o.WriteLine();
        }

        /// <summary>
        /// Write a node in DOT format
        /// </summary>
        private void WriteNode(T n, TextWriter o)
        {
            o.Write(IdOf[n]);
            o.Write(" [");
            o.Write(" label = ");
            WriteAttribute(NodeLabel(n), o);
            WriteAttributeList(NodeAttributes[n], " ", null, o);
            o.WriteLine("];");
        }
        #endregion
        
        #region Edges
        /// <summary>
        /// The set of all edges in the graph
        /// </summary>
        public ISet<Edge> Edges => edges;

        private readonly HashSet<Edge> edges = new HashSet<Edge>();

        /// <summary>
        /// Represents an edge in a GraphViz graph
        /// </summary>
        public class Edge
        {
            /// <summary>
            /// Node this edge originates at.
            /// If this is an undirected edge, then StartNode and EndNode are interchangeable
            /// </summary>
            public readonly T StartNode;
            /// <summary>
            /// Node this edge ends at.
            /// If this is an undirected edge, then StartNode and EndNode are interchangeable
            /// </summary>
            public readonly T EndNode;
            /// <summary>
            /// Whether this is a directed edge or not
            /// </summary>
            public readonly bool Directed;
            /// <summary>
            /// Text to mark this edge with, or null, if edge is to be unlabeled.
            /// </summary>
            public readonly string? Label;
            /// <summary>
            /// Attributes to be applied to this edge
            /// </summary>
            public Dictionary<string, object>? Attributes;

            /// <summary>
            /// Make an edge
            /// </summary>
            /// <param name="startNode">Node the edge should start from.</param>
            /// <param name="endNode">Node the edge should end at</param>
            /// <param name="directed">Whether the edge is directed.  If not, startNode and endNode can be switched.</param>
            /// <param name="label">Text to display along with the edge, or null</param>
            /// <param name="attributes">Attributes to apply to the edge, if any</param>
            public Edge(T startNode, T endNode, bool directed = true, string? label = null, Dictionary<string,object>? attributes = null)
            {
                StartNode = startNode;
                this.EndNode = endNode;
                Directed = directed;
                Label = label;
                Attributes = attributes;
            }

            /// <summary>
            /// Copy the attributes from the specified edge into this edge's attributes
            /// </summary>
            /// <param name="e"></param>
            public void AddAttributes(Edge e) => AddAttributes(e.Attributes);

            /// <summary>
            /// Add the specified attributes to this edge
            /// </summary>
            /// <param name="attributes"></param>
            public void AddAttributes(IEnumerable<KeyValuePair<string, object>>? attributes)
            {
                if (attributes == null) return;
                Attributes ??= new Dictionary<string, object>();
                foreach (var pair in attributes) Attributes[pair.Key] = pair.Value;
            }

            /// <summary>
            /// True if two edges are the same.
            /// Edges are the same if they have the same start and end nodes, directedness, and label.
            /// If the node isn't directed, the the order of start and end nodes doesn't matter.
            /// Attributes are ignored for purposes of equality.
            /// </summary>
            public static bool operator ==(Edge a, Edge b)
            {
                if (a.Directed != b.Directed || !Equals(a.Label, b.Label))
                    return false;
                if (a.Directed)
                    return Comparer<T>.Default.Equals(a.StartNode, b.StartNode)
                           && Comparer<T>.Default.Equals(a.EndNode, b.EndNode);
                return (Comparer<T>.Default.Equals(a.StartNode, b.StartNode)
                        && Comparer<T>.Default.Equals(a.EndNode, b.EndNode))
                       || (Comparer<T>.Default.Equals(a.StartNode, b.EndNode)
                           && Comparer<T>.Default.Equals(a.EndNode, b.StartNode));
            }

            /// <summary>
            /// True if the nodes are not the same
            /// </summary>
            public static bool operator !=(Edge a, Edge b) => !(a == b);

            /// <summary>
            /// Make an edge from a tuple
            /// </summary>
            public static implicit operator Edge((T start, T end) e) => new Edge(e.Item1, e.Item2);
            /// <summary>
            /// Make an edge from a tuple
            /// </summary>
            public static implicit operator Edge((T start, T end, string label) e) => new Edge(e.Item1, e.Item2, true, e.Item3);
            /// <summary>
            /// Make an edge from a tuple
            /// </summary>
            public static implicit operator Edge((T start, T end, string label, bool directed) e) => new Edge(e.Item1, e.Item2, e.Item4, e.Item3);

            /// <summary>
            /// True if obj is an edge that is == to this edge
            /// Edges are the same if they have the same start and end nodes, directedness, and label.
            /// If the node isn't directed, the the order of start and end nodes doesn't matter.
            /// Attributes are ignored for purposes of equality.
            /// </summary>
            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is Edge e && this == e;
            }

            /// <summary>
            /// Return a hash code for the edge.  If the edge is directed, then it is symmetric in the start and end nodes
            /// </summary>
            public override int GetHashCode()
            {
                // We need the hash to be symmetric in InNode and OutNode in case the edge is undirected.
                return HashCode.Combine(StartNode!.GetHashCode() + EndNode!.GetHashCode(), Directed, Label);
            }
        }
        #endregion

        #region Clusters
        /// <summary>
        /// A cluster of nodes to be grouped together during rendering
        /// </summary>
        public class Cluster
        {
            /// <summary>
            /// Name of the cluster
            /// </summary>
            public string Name;
            /// <summary>
            /// Nodes in the cluster
            /// </summary>
            public HashSet<T> Nodes = new HashSet<T>();
            /// <summary>
            /// Attributes rendering for this cluster
            /// </summary>
            public Dictionary<string, object> Attributes = new Dictionary<string, object>();

            /// <summary>
            /// Other clusters to be rendered inside this cluster
            /// </summary>
            public List<Cluster> SubClusters = new List<Cluster>();

            internal Cluster(string name, Cluster? parent = null)
            {
                Name = name;
                parent?.SubClusters.Add(this);
            }
        }

        /// <summary>
        /// Clusters for this graph that are not inside other clusters
        /// </summary>
        private readonly List<Cluster> topLevelClusters = new List<Cluster>();

        /// <summary>
        /// Add a cluster to the graph
        /// </summary>
        /// <param name="name">Name to give to the cluster</param>
        /// <param name="parent">Cluster inside of which to render this cluster, or null</param>
        /// <returns></returns>
        public Cluster MakeCluster(string name, Cluster? parent = null)
        {
            var c = new Cluster(name, parent);
            if (parent == null)
                topLevelClusters.Add(c);
            return c;
        }
        #endregion
    }
}
