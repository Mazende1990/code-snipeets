using System;
using System.Collections.Generic;
using DataStructures.DisjointSet;

namespace Algorithms.Graph.MinimumSpanningTree
{
    /// <summary>
    ///     Implementation of Kruskal's algorithm to find minimum spanning trees/forests.
    /// </summary>
    /// <remarks>
    ///     Kruskal's algorithm is a greedy algorithm that finds a minimum spanning tree/forest 
    ///     for undirected graphs. It works with both connected and unconnected graphs.
    ///     Time complexity: O(E log V) where E is edges and V is vertices.
    ///     References:
    ///     https://en.wikipedia.org/wiki/Kruskal%27s_algorithm
    ///     https://www.personal.kent.edu/~rmuhamma/Algorithms/MyAlgorithms/GraphAlgor/primAlgor.htm
    /// </remarks>
    public static class Kruskal
    {
        /// <summary>
        ///     Finds minimum spanning tree/forest from an adjacency matrix representation.
        /// </summary>
        /// <param name="adjacencyMatrix">Square symmetric matrix representing edge weights.</param>
        /// <returns>Adjacency matrix of the minimum spanning tree/forest.</returns>
        public static float[,] Solve(float[,] adjacencyMatrix)
        {
            ValidateAdjacencyMatrix(adjacencyMatrix);
            var edges = GetEdgesFromMatrix(adjacencyMatrix);
            var mstEdges = FindMstEdges(adjacencyMatrix.GetLength(0), edges);
            return BuildMstMatrix(adjacencyMatrix, mstEdges);
        }

        /// <summary>
        ///     Finds minimum spanning tree/forest from an adjacency list representation.
        /// </summary>
        /// <param name="adjacencyList">Adjacency list representation of the graph.</param>
        /// <returns>Adjacency list of the minimum spanning tree/forest.</returns>
        public static Dictionary<int, float>[] Solve(Dictionary<int, float>[] adjacencyList)
        {
            ValidateAdjacencyList(adjacencyList);
            var edges = GetEdgesFromList(adjacencyList);
            var mstEdges = FindMstEdges(adjacencyList.Length, edges);
            return BuildMstList(adjacencyList, mstEdges);
        }

        #region Private Helper Methods

        private static void ValidateAdjacencyMatrix(float[,] matrix)
        {
            if (matrix.GetLength(0) != matrix.GetLength(1))
            {
                throw new ArgumentException("Adjacency matrix must be square.");
            }

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = i + 1; j < matrix.GetLength(1); j++)
                {
                    if (Math.Abs(matrix[i, j] - matrix[j, i]) > 1e-6)
                    {
                        throw new ArgumentException("Adjacency matrix must be symmetric (undirected graph).");
                    }
                }
            }
        }

        private static void ValidateAdjacencyList(Dictionary<int, float>[] adjacencyList)
        {
            for (int i = 0; i < adjacencyList.Length; i++)
            {
                foreach (var edge in adjacencyList[i])
                {
                    if (!adjacencyList[edge.Key].TryGetValue(i, out float weight) || 
                        Math.Abs(edge.Value - weight) > 1e-6)
                    {
                        throw new ArgumentException("Graph must be undirected.");
                    }
                }
            }
        }

        private static List<(int Node1, int Node2, float Weight)> GetEdgesFromMatrix(float[,] matrix)
        {
            var edges = new List<(int, int, float)>();
            int nodeCount = matrix.GetLength(0);

            for (int i = 0; i < nodeCount; i++)
            {
                for (int j = i + 1; j < nodeCount; j++)
                {
                    if (float.IsFinite(matrix[i, j]))
                    {
                        edges.Add((i, j, matrix[i, j]));
                    }
                }
            }

            return edges;
        }

        private static List<(int Node1, int Node2, float Weight)> GetEdgesFromList(Dictionary<int, float>[] adjacencyList)
        {
            var edges = new List<(int, int, float)>();
            
            for (int i = 0; i < adjacencyList.Length; i++)
            {
                foreach (var edge in adjacencyList[i])
                {
                    // Avoid duplicate edges (since graph is undirected)
                    if (i < edge.Key)
                    {
                        edges.Add((i, edge.Key, edge.Value));
                    }
                }
            }

            return edges;
        }

        private static (int, int)[] FindMstEdges(int nodeCount, List<(int Node1, int Node2, float Weight)> edges)
        {
            var set = new DisjointSet<int>();
            var nodes = new Node<int>[nodeCount];
            var mstEdges = new List<(int, int)>();

            // Initialize disjoint set with all nodes
            for (int i = 0; i < nodeCount; i++)
            {
                nodes[i] = set.MakeSet(i);
            }

            // Sort edges by weight
            edges.Sort((a, b) => a.Weight.CompareTo(b.Weight));

            // Kruskal's algorithm - add edges if they don't form cycles
            foreach (var edge in edges)
            {
                if (set.FindSet(nodes[edge.Node1]) != set.FindSet(nodes[edge.Node2]))
                {
                    set.UnionSet(nodes[edge.Node1], nodes[edge.Node2]);
                    mstEdges.Add((edge.Node1, edge.Node2));
                }
            }

            return mstEdges.ToArray();
        }

        private static float[,] BuildMstMatrix(float[,] originalMatrix, (int, int)[] mstEdges)
        {
            int nodeCount = originalMatrix.GetLength(0);
            var mst = new float[nodeCount, nodeCount];

            // Initialize matrix with infinity (no edges)
            for (int i = 0; i < nodeCount; i++)
            {
                for (int j = 0; j < nodeCount; j++)
                {
                    mst[i, j] = float.PositiveInfinity;
                }
            }

            // Add MST edges
            foreach (var (node1, node2) in mstEdges)
            {
                float weight = originalMatrix[node1, node2];
                mst[node1, node2] = weight;
                mst[node2, node1] = weight;
            }

            return mst;
        }

        private static Dictionary<int, float>[] BuildMstList(Dictionary<int, float>[] originalList, (int, int)[] mstEdges)
        {
            var mst = new Dictionary<int, float>[originalList.Length];

            // Initialize adjacency lists
            for (int i = 0; i < mst.Length; i++)
            {
                mst[i] = new Dictionary<int, float>();
            }

            // Add MST edges
            foreach (var (node1, node2) in mstEdges)
            {
                float weight = originalList[node1][node2];
                mst[node1].Add(node2, weight);
                mst[node2].Add(node1, weight);
            }

            return mst;
        }

        #endregion
    }
}