package java.s3;

import java.util.*;

public class BellmanFord {

    private int vertexCount, edgeCount;
    private Edge[] edges;
    private int edgeIndex = 0;

    public BellmanFord(int vertexCount, int edgeCount) {
        this.vertexCount = vertexCount;
        this.edgeCount = edgeCount;
        this.edges = new Edge[edgeCount];
    }

    // Inner class to represent an edge
    class Edge {
        int from, to, weight;

        public Edge(int from, int to, int weight) {
            this.from = from;
            this.to = to;
            this.weight = weight;
        }
    }

    // Adds a unidirectional edge to the graph
    public void addEdge(int from, int to, int weight) {
        edges[edgeIndex++] = new Edge(from, to, weight);
    }

    public Edge[] getEdgeArray() {
        return edges;
    }

    // Utility method to print the path from source to a given vertex
    private void printPath(int[] parent, int vertex) {
        if (parent[vertex] == -1) return;
        printPath(parent, parent[vertex]);
        System.out.print(vertex + " ");
    }

    // Interactive method for testing the Bellman-Ford algorithm
    public void go() {
        Scanner sc = new Scanner(System.in);

        System.out.println("Enter number of vertices and edges:");
        int v = sc.nextInt();
        int e = sc.nextInt();
        Edge[] inputEdges = new Edge[e];

        System.out.println("Input edges (from to weight):");
        for (int i = 0; i < e; i++) {
            int from = sc.nextInt();
            int to = sc.nextInt();
            int weight = sc.nextInt();
            inputEdges[i] = new Edge(from, to, weight);
        }

        int source = 0;
        bellmanFord(v, e, source, inputEdges);

        sc.close();
    }

    // Bellman-Ford core logic: calculates shortest distances and detects negative weight cycles
    private void bellmanFord(int vertexCount, int edgeCount, int source, Edge[] edges) {
        int[] distance = new int[vertexCount];
        int[] parent = new int[vertexCount];
        boolean hasNegativeCycle = false;

        Arrays.fill(distance, Integer.MAX_VALUE);
        distance[source] = 0;
        parent[source] = -1;

        for (int i = 0; i < vertexCount - 1; i++) {
            for (Edge edge : edges) {
                if (distance[edge.from] != Integer.MAX_VALUE &&
                    distance[edge.to] > distance[edge.from] + edge.weight) {
                    distance[edge.to] = distance[edge.from] + edge.weight;
                    parent[edge.to] = edge.from;
                }
            }
        }

        for (Edge edge : edges) {
            if (distance[edge.from] != Integer.MAX_VALUE &&
                distance[edge.to] > distance[edge.from] + edge.weight) {
                hasNegativeCycle = true;
                System.out.println("Negative cycle detected.");
                break;
            }
        }

        if (!hasNegativeCycle) {
            System.out.println("Shortest distances from source:");
            for (int i = 0; i < vertexCount; i++) {
                System.out.println("Vertex " + i + ": " + distance[i]);
            }

            System.out.println("\nPaths from source:");
            for (int i = 0; i < vertexCount; i++) {
                System.out.print("Path to " + i + ": 0 ");
                printPath(parent, i);
                System.out.println();
            }
        }
    }

    // Method for external use: shows path from source to a specific destination
    public void show(int source, int destination, Edge[] edges) {
        int[] distance = new int[vertexCount];
        int[] parent = new int[vertexCount];
        boolean hasNegativeCycle = false;

        Arrays.fill(distance, Integer.MAX_VALUE);
        distance[source] = 0;
        parent[source] = -1;

        for (int i = 0; i < vertexCount - 1; i++) {
            for (Edge edge : edges) {
                if (distance[edge.from] != Integer.MAX_VALUE &&
                    distance[edge.to] > distance[edge.from] + edge.weight) {
                    distance[edge.to] = distance[edge.from] + edge.weight;
                    parent[edge.to] = edge.from;
                }
            }
        }

        for (Edge edge : edges) {
            if (distance[edge.from] != Integer.MAX_VALUE &&
                distance[edge.to] > distance[edge.from] + edge.weight) {
                hasNegativeCycle = true;
                System.out.println("Negative cycle detected.");
                break;
            }
        }

        if (!hasNegativeCycle) {
            System.out.println("Shortest distance from " + source + " to " + destination + ": " + distance[destination]);
            System.out.print("Path: " + source + " ");
            printPath(parent, destination);
            System.out.println();
        }
    }

    public static void main(String[] args) {
        BellmanFord graph = new BellmanFord(0, 0);
        graph.go(); // Start the interactive session
    }
}
