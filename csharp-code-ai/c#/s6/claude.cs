using System;
using System.Collections.Generic;
using System.Linq;
using DataStructures.DisjointSet;

namespace Algorithms.Graph.MinimumSpanningTree
{
    /// <summary>
    /// Implements Kruskal's algorithm for finding the minimum spanning tree or forest of an undirected graph.
    /// </summary>
    /// <remarks>
    /// Kruskal's algorithm is a greedy approach to finding the minimum spanning tree.
    /// It works on both connected and unconnected graphs with a time complexity of O(E log V),
    /// where E is the number of edges and V is the number of vertices.
    /// </remarks>
    public static class KruskalMinimumSpanningTree
    {
        /// <summary>
        /// Finds the minimum spanning tree/forest using an adjacency matrix representation.
        /// </summary>
        /// <param name="adjacencyMatrix">The graph represented as an adjacency matrix.</param>
        /// <returns>Adjacency matrix of the minimum spanning tree/forest.</returns>
        public static float[,] FindMinimumSpanningTree(float[,] adjacencyMatrix)
        {
            ValidateUndirectedGraph(adjacencyMatrix);

            int nodeCount = adjacencyMatrix.GetLength(0);
            var (edgeWeights, nodeConnections) = ExtractEdgesFromAdjacencyMatrix(adjacencyMatrix);
            var minimumSpanningTreeEdges = FindMinimumSpanningTreeEdges(nodeCount, edgeWeights, nodeConnections);

            return CreateMinimumSpanningTreeMatrix(nodeCount, adjacencyMatrix, minimumSpanningTreeEdges);
        }

        /// <summary>
        /// Finds the minimum spanning tree/forest using an adjacency list representation.
        /// </summary>
        /// <param name="adjacencyList">The graph represented as an adjacency list.</param>
        /// <returns>Adjacency list of the minimum spanning tree/forest.</returns>
        public static Dictionary<int, float>[] FindMinimumSpanningTree(Dictionary<int, float>[] adjacencyList)
        {
            ValidateUndirectedGraph(adjacencyList);

            int nodeCount = adjacencyList.Length;
            var (edgeWeights, nodeConnections) = ExtractEdgesFromAdjacencyList(adjacencyList);
            var minimumSpanningTreeEdges = FindMinimumSpanningTreeEdges(nodeCount, edgeWeights, nodeConnections);

            return CreateMinimumSpanningTreeList(nodeCount, adjacencyList, minimumSpanningTreeEdges);
        }

        /// <summary>
        /// Extracts edges and their weights from an adjacency matrix.
        /// </summary>
        private static (float[] edgeWeights, (int, int)[] nodeConnections) ExtractEdgesFromAdjacencyMatrix(float[,] adjacencyMatrix)
        {
            var edgeWeightList = new List<float>();
            var nodeConnectList = new List<(int, int)>();

            int nodeCount = adjacencyMatrix.GetLength(0);
            for (int i = 0; i < nodeCount - 1; i++)
            {
                for (int j = i + 1; j < nodeCount; j++)
                {
                    if (float.IsFinite(adjacencyMatrix[i, j]))
                    {
                        edgeWeightList.Add(adjacencyMatrix[i, j]);
                        nodeConnectList.Add((i, j));
                    }
                }
            }

            return (edgeWeightList.ToArray(), nodeConnectList.ToArray());
        }

        /// <summary>
        /// Extracts edges and their weights from an adjacency list.
        /// </summary>
        private static (float[] edgeWeights, (int, int)[] nodeConnections) ExtractEdgesFromAdjacencyList(Dictionary<int, float>[] adjacencyList)
        {
            var edgeWeightList = new List<float>();
            var nodeConnectList = new List<(int, int)>();

            for (int i = 0; i < adjacencyList.Length; i++)
            {
                foreach (var (node, weight) in adjacencyList[i])
                {
                    edgeWeightList.Add(weight);
                    nodeConnectList.Add((i, node));
                }
            }

            return (edgeWeightList.ToArray(), nodeConnectList.ToArray());
        }

        /// <summary>
        /// Finds the edges of the minimum spanning tree using a disjoint set data structure.
        /// </summary>
        private static (int, int)[] FindMinimumSpanningTreeEdges(
            int nodeCount, 
            float[] edgeWeights, 
            (int, int)[] nodeConnections)
        {
            var disjointSet = new DisjointSet<int>();
            var nodes = new Node<int>[nodeCount];
            var minimumSpanningTreeEdges = new List<(int, int)>();

            // Initialize disjoint set with nodes
            for (int i = 0; i < nodeCount; i++)
            {
                nodes[i] = disjointSet.MakeSet(i);
            }

            // Sort edges by weight
            Array.Sort(edgeWeights, nodeConnections);

            // Process edges in ascending order of weight
            foreach (var (node1, node2) in nodeConnections)
            {
                if (disjointSet.FindSet(nodes[node1]) != disjointSet.FindSet(nodes[node2]))
                {
                    disjointSet.UnionSet(nodes[node1], nodes[node2]);
                    minimumSpanningTreeEdges.Add((node1, node2));
                }
            }

            return minimumSpanningTreeEdges.ToArray();
        }

        /// <summary>
        /// Creates a minimum spanning tree matrix from the found edges.
        /// </summary>
        private static float[,] CreateMinimumSpanningTreeMatrix(
            int nodeCount, 
            float[,] originalAdjacencyMatrix, 
            (int, int)[] minimumSpanningTreeEdges)
        {
            var minimumSpanningTree = new float[nodeCount, nodeCount];

            // Initialize matrix with infinity
            for (int i = 0; i < nodeCount; i++)
            {
                minimumSpanningTree[i, i] = float.PositiveInfinity;

                for (int j = i + 1; j < nodeCount; j++)
                {
                    minimumSpanningTree[i, j] = float.PositiveInfinity;
                    minimumSpanningTree[j, i] = float.PositiveInfinity;
                }
            }

            // Add edges to the minimum spanning tree
            foreach (var (node1, node2) in minimumSpanningTreeEdges)
            {
                float edgeWeight = originalAdjacencyMatrix[node1, node2];
                minimumSpanningTree[node1, node2] = edgeWeight;
                minimumSpanningTree[node2, node1] = edgeWeight;
            }

            return minimumSpanningTree;
        }

        /// <summary>
        /// Creates a minimum spanning tree adjacency list from the found edges.
        /// </summary>
        private static Dictionary<int, float>[] CreateMinimumSpanningTreeList(
            int nodeCount, 
            Dictionary<int, float>[] originalAdjacencyList, 
            (int, int)[] minimumSpanningTreeEdges)
        {
            var minimumSpanningTree = new Dictionary<int, float>[nodeCount];
            
            // Initialize adjacency lists
            for (int i = 0; i < nodeCount; i++)
            {
                minimumSpanningTree[i] = new Dictionary<int, float>();
            }

            // Add edges to the minimum spanning tree
            foreach (var (node1, node2) in minimumSpanningTreeEdges)
            {
                float edgeWeight = originalAdjacencyList[node1][node2];
                minimumSpanningTree[node1][node2] = edgeWeight;
                minimumSpanningTree[node2][node1] = edgeWeight;
            }

            return minimumSpanningTree;
        }

        /// <summary>
        /// Validates that the adjacency matrix represents an undirected graph.
        /// </summary>
        private static void ValidateUndirectedGraph(float[,] adjacencyMatrix)
        {
            if (adjacencyMatrix.GetLength(0) != adjacencyMatrix.GetLength(1))
            {
                throw new ArgumentException("Adjacency matrix must be square!");
            }

            for (int i = 0; i < adjacencyMatrix.GetLength(0) - 1; i++)
            {
                for (int j = i + 1; j < adjacencyMatrix.GetLength(1); j++)
                {
                    if (Math.Abs(adjacencyMatrix[i, j] - adjacencyMatrix[j, i]) > 1e-6)
                    {
                        throw new ArgumentException("Adjacency matrix must be symmetric!");
                    }
                }
            }
        }

        /// <summary>
        /// Validates that the adjacency list represents an undirected graph.
        /// </summary>
        private static void ValidateUndirectedGraph(Dictionary<int, float>[] adjacencyList)
        {
            for (int i = 0; i < adjacencyList.Length; i++)
            {
                foreach (var edge in adjacencyList[i])
                {
                    if (!adjacencyList[edge.Key].ContainsKey(i) || 
                        Math.Abs(edge.Value - adjacencyList[edge.Key][i]) > 1e-6)
                    {
                        throw new ArgumentException("Adjacency list must represent an undirected graph!");
                    }
                }
            }
        }
    }
}