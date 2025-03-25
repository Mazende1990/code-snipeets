using System;
using System.Collections.Generic;
using DataStructures.DisjointSet;

namespace Algorithms.Graph.MinimumSpanningTree
{
    /// <summary>
    ///     Kruskal's algorithm for finding the minimum spanning tree (or forest) of an undirected graph.
    ///     Suitable for disconnected graphs. Time complexity: O(E log V).
    /// </summary>
    public static class Kruskal
    {
        public static float[,] Solve(float[,] adjacencyMatrix)
        {
            ValidateAdjacencyMatrix(adjacencyMatrix);

            int nodeCount = adjacencyMatrix.GetLength(0);
            var disjointSet = new DisjointSet<int>();
            var nodes = new Node<int>[nodeCount];
            var weights = new List<float>();
            var edges = new List<(int, int)>();

            for (int i = 0; i < nodeCount; i++)
                nodes[i] = disjointSet.MakeSet(i);

            for (int i = 0; i < nodeCount - 1; i++)
            {
                for (int j = i + 1; j < nodeCount; j++)
                {
                    if (float.IsFinite(adjacencyMatrix[i, j]))
                    {
                        weights.Add(adjacencyMatrix[i, j]);
                        edges.Add((i, j));
                    }
                }
            }

            var mstEdges = GetMstEdges(disjointSet, nodes, weights.ToArray(), edges.ToArray());

            var mstMatrix = CreateEmptyMatrix(nodeCount, float.PositiveInfinity);
            foreach (var (u, v) in mstEdges)
            {
                float weight = adjacencyMatrix[u, v];
                mstMatrix[u, v] = weight;
                mstMatrix[v, u] = weight;
            }

            return mstMatrix;
        }

        public static Dictionary<int, float>[] Solve(Dictionary<int, float>[] adjacencyList)
        {
            ValidateAdjacencyList(adjacencyList);

            int nodeCount = adjacencyList.Length;
            var disjointSet = new DisjointSet<int>();
            var nodes = new Node<int>[nodeCount];
            var weights = new List<float>();
            var edges = new List<(int, int)>();

            for (int i = 0; i < nodeCount; i++)
            {
                nodes[i] = disjointSet.MakeSet(i);
                foreach (var (neighbor, weight) in adjacencyList[i])
                {
                    weights.Add(weight);
                    edges.Add((i, neighbor));
                }
            }

            var mstEdges = GetMstEdges(disjointSet, nodes, weights.ToArray(), edges.ToArray());

            var mstList = new Dictionary<int, float>[nodeCount];
            for (int i = 0; i < nodeCount; i++)
                mstList[i] = new Dictionary<int, float>();

            foreach (var (u, v) in mstEdges)
            {
                float weight = adjacencyList[u][v];
                mstList[u][v] = weight;
                mstList[v][u] = weight;
            }

            return mstList;
        }

        private static void ValidateAdjacencyMatrix(float[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            if (rows != cols)
                throw new ArgumentException("Adjacency matrix must be square.");

            for (int i = 0; i < rows - 1; i++)
            {
                for (int j = i + 1; j < cols; j++)
                {
                    if (Math.Abs(matrix[i, j] - matrix[j, i]) > 1e-6)
                        throw new ArgumentException("Adjacency matrix must be symmetric.");
                }
            }
        }

        private static void ValidateAdjacencyList(Dictionary<int, float>[] list)
        {
            for (int i = 0; i < list.Length; i++)
            {
                foreach (var (neighbor, weight) in list[i])
                {
                    if (!list[neighbor].ContainsKey(i) || Math.Abs(weight - list[neighbor][i]) > 1e-6)
                        throw new ArgumentException("Adjacency list must be symmetric (undirected).");
                }
            }
        }

        private static (int, int)[] GetMstEdges(
            DisjointSet<int> set,
            Node<int>[] nodes,
            float[] weights,
            (int, int)[] connections)
        {
            Array.Sort(weights, connections);
            var mstEdges = new List<(int, int)>();

            foreach (var (u, v) in connections)
            {
                if (set.FindSet(nodes[u]) != set.FindSet(nodes[v]))
                {
                    set.UnionSet(nodes[u], nodes[v]);
                    mstEdges.Add((u, v));
                }
            }

            return mstEdges.ToArray();
        }

        private static float[,] CreateEmptyMatrix(int size, float defaultValue)
        {
            var matrix = new float[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                    matrix[i, j] = (i == j) ? float.PositiveInfinity : defaultValue;
            }
            return matrix;
        }
    }
}
