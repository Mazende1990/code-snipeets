using System;
using System.Collections.Generic;
using DataStructures.DisjointSet;

namespace Algorithms.Graph.MinimumSpanningTree
{
    /// <summary>
    /// Algorithm to determine the minimum spanning forest of an undirected graph.
    /// </summary>
    /// <remarks>
    /// Kruskal's algorithm is a greedy algorithm that can determine the
    /// minimum spanning tree or minimum spanning forest of any undirected
    /// graph. Unlike Prim's algorithm, Kruskal's algorithm will work on
    /// graphs that are unconnected. This algorithm will always have a
    /// running time of O(E log V) where E is the number of edges and V is
    /// the number of vertices/nodes.
    /// More information: https://en.wikipedia.org/wiki/Kruskal%27s_algorithm .
    /// Pseudocode and analysis: https://www.personal.kent.edu/~rmuhamma/Algorithms/MyAlgorithms/GraphAlgor/primAlgor.htm .
    /// </remarks>
    public static class Kruskal
    {
        /// <summary>
        /// Determine the minimum spanning tree/forest of the given graph represented as an adjacency matrix.
        /// </summary>
        /// <param name="adjacencyMatrix">Adjacency matrix representing the graph.</param>
        /// <returns>Adjacency matrix of the minimum spanning tree/forest.</returns>
        public static float[,] Solve(float[,] adjacencyMatrix)
        {
            ValidateGraph(adjacencyMatrix);
            int numNodes = adjacencyMatrix.GetLength(0);
            var disjointSet = new DisjointSet<int>();
            var nodes = new Node<int>[numNodes];
            var edges = new List<(int, int)>();
            var allEdges = ExtractEdges(adjacencyMatrix);

            // Initialize disjoint set
            for (int i = 0; i < numNodes; i++)
            {
                nodes[i] = disjointSet.MakeSet(i);
            }

            // Sort edges by weight
            allEdges.Sort((edge1, edge2) => edge1.weight.CompareTo(edge2.weight));

            // Apply Kruskal's algorithm
            foreach (var edge in allEdges)
            {
                if (disjointSet.FindSet(nodes[edge.node1]) != disjointSet.FindSet(nodes[edge.node2]))
                {
                    disjointSet.UnionSet(nodes[edge.node1], nodes[edge.node2]);
                    edges.Add((edge.node1, edge.node2));
                }
            }

            return BuildMinimumSpanningTreeMatrix(adjacencyMatrix, edges);
        }

        /// <summary>
        /// Determine the minimum spanning tree/forest of the given graph represented as an adjacency list.
        /// </summary>
        /// <param name="adjacencyList">Adjacency list representing the graph.</param>
        /// <returns>Adjacency list of the minimum spanning tree/forest.</returns>
        public static Dictionary<int, float>[] Solve(Dictionary<int, float>[] adjacencyList)
        {
            ValidateGraph(adjacencyList);
            int numNodes = adjacencyList.Length;
            var disjointSet = new DisjointSet<int>();
            var nodes = new Node<int>[numNodes];
            var edges = new List<(int, int)>();
            var allEdges = ExtractEdges(adjacencyList);

            // Initialize disjoint set
            for (int i = 0; i < numNodes; i++)
            {
                nodes[i] = disjointSet.MakeSet(i);
            }

            // Sort edges by weight
            allEdges.Sort((edge1, edge2) => edge1.weight.CompareTo(edge2.weight));

            // Apply Kruskal's algorithm
            foreach (var edge in allEdges)
            {
                if (disjointSet.FindSet(nodes[edge.node1]) != disjointSet.FindSet(nodes[edge.node2]))
                {
                    disjointSet.UnionSet(nodes[edge.node1], nodes[edge.node2]);
                    edges.Add((edge.node1, edge.node2));
                }
            }

            return BuildMinimumSpanningTreeList(adjacencyList, edges);
        }

        private static List<(int node1, int node2, float weight)> ExtractEdges(float[,] adjacencyMatrix)
        {
            int numNodes = adjacencyMatrix.GetLength(0);
            var edges = new List<(int, int, float)>();

            for (int i = 0; i < numNodes - 1; i++)
            {
                for (int j = i + 1; j < numNodes; j++)
                {
                    if (float.IsFinite(adjacencyMatrix[i, j]))
                    {
                        edges.Add((i, j, adjacencyMatrix[i, j]));
                    }
                }
            }
            return edges;
        }

        private static List<(int node1, int node2, float weight)> ExtractEdges(Dictionary<int, float>[] adjacencyList)
        {
            var edges = new List<(int, int, float)>();
            for (int i = 0; i < adjacencyList.Length; i++)
            {
                foreach (var edge in adjacencyList[i])
                {
                    edges.Add((i, edge.Key, edge.Value));
                }
            }
            return edges;
        }

        private static float[,] BuildMinimumSpanningTreeMatrix(float[,] originalMatrix, List<(int, int)> mstEdges)
        {
            int numNodes = originalMatrix.GetLength(0);
            var mst = new float[numNodes, numNodes];

            for (int i = 0; i < numNodes; i++)
            {
                for (int j = 0; j < numNodes; j++)
                {
                    mst[i, j] = float.PositiveInfinity;
                }
                mst[i, i] = 0;
            }

            foreach (var (node1, node2) in mstEdges)
            {
                mst[node1, node2] = originalMatrix[node1, node2];
                mst[node2, node1] = originalMatrix[node1, node2];
            }

            return mst;
        }

        private static Dictionary<int, float>[] BuildMinimumSpanningTreeList(Dictionary<int, float>[] originalList, List<(int, int)> mstEdges)
        {
            int numNodes = originalList.Length;
            var mst = new Dictionary<int, float>[numNodes];

            for (int i = 0; i < numNodes; i++)
            {
                mst[i] = new Dictionary<int, float>();
            }

            foreach (var (node1, node2) in mstEdges)
            {
                mst[node1].Add(node2, originalList[node1][node2]);
                mst[node2].Add(node1, originalList[node1][node2]);
            }

            return mst;
        }

        private static void ValidateGraph(float[,] adj)
        {
            if (adj.GetLength(0) != adj.GetLength(1))
            {
                throw new ArgumentException("Matrix must be square!");
            }

            for (int i = 0; i < adj.GetLength(0) - 1; i++)
            {
                for (int j = i + 1; j < adj.GetLength(1); j++)
                {
                    if (Math.Abs(adj[i, j] - adj[j, i]) > 1e-6)
                    {
                        throw new ArgumentException("Matrix must be symmetric!");
                    }
                }
            }
        }

        private static void ValidateGraph(Dictionary<int, float>[] adj)
        {
            for (int i = 0; i < adj.Length; i++)
            {
                foreach (var edge in adj[i])
                {
                    if (!adj[edge.Key].ContainsKey(i) || Math.Abs(edge.Value - adj[edge.Key][i]) > 1e-6)
                    {
                        throw new ArgumentException("Graph must be undirected!");
                    }
                }
            }
        }
    }
}