package java.s3;

import java.util.Arrays;
import java.util.Scanner;

class BellmanFordGemini {

    private int vertexCount;
    private int edgeCount;
    private Edge[] edges;
    private int edgeIndex = 0;

    BellmanFordGemini(int vertexCount, int edgeCount) {
        this.vertexCount = vertexCount;
        this.edgeCount = edgeCount;
        this.edges = new Edge[edgeCount];
    }

    static class Edge {
        int source;
        int destination;
        int weight;

        Edge(int source, int destination, int weight) {
            this.source = source;
            this.destination = destination;
            this.weight = weight;
        }
    }

    private void printPath(int[] parent, int vertex) {
        if (parent[vertex] == -1) {
            return;
        }
        printPath(parent, parent[vertex]);
        System.out.print(vertex + " ");
    }

    public static void main(String[] args) {
        BellmanFordGemini bellmanFord = new BellmanFordGemini(0, 0); // Dummy object for go()
        bellmanFord.runInteractive();
    }

    public void runInteractive() {
        Scanner scanner = new Scanner(System.in);
        System.out.println("Enter the number of vertices and edges:");
        vertexCount = scanner.nextInt();
        edgeCount = scanner.nextInt();
        edges = new Edge[edgeCount];

        System.out.println("Enter the edges (source destination weight):");
        for (int i = 0; i < edgeCount; i++) {
            int source = scanner.nextInt();
            int destination = scanner.nextInt();
            int weight = scanner.nextInt();
            edges[i] = new Edge(source, destination, weight);
        }

        int[] distances = new int[vertexCount];
        int[] parent = new int[vertexCount];
        Arrays.fill(distances, Integer.MAX_VALUE);
        distances[0] = 0;
        parent[0] = -1;

        if (detectNegativeCycleAndCalculateDistances(distances, parent)) {
            System.out.println("Distances from source (0):");
            for (int i = 0; i < vertexCount; i++) {
                System.out.println(i + " " + distances[i]);
            }

            System.out.println("Paths from source (0):");
            for (int i = 0; i < vertexCount; i++) {
                System.out.print("0 ");
                printPath(parent, i);
                System.out.println();
            }
        }

        scanner.close();
    }

    public void showShortestPath(int source, int destination, Edge[] edges) {
        vertexCount = calculateMaxVertex(edges) + 1;
        edgeCount = edges.length;
        this.edges = edges;

        int[] distances = new int[vertexCount];
        int[] parent = new int[vertexCount];
        Arrays.fill(distances, Integer.MAX_VALUE);
        distances[source] = 0;
        parent[source] = -1;

        if (detectNegativeCycleAndCalculateDistances(distances, parent)) {
            System.out.println("Distance from " + source + " to " + destination + ": " + distances[destination]);
            System.out.println("Path followed:");
            System.out.print(source + " ");
            printPath(parent, destination);
            System.out.println();
        }
    }

    private boolean detectNegativeCycleAndCalculateDistances(int[] distances, int[] parent) {
        for (int i = 0; i < vertexCount - 1; i++) {
            for (Edge edge : edges) {
                if (distances[edge.source] != Integer.MAX_VALUE && distances[edge.destination] > distances[edge.source] + edge.weight) {
                    distances[edge.destination] = distances[edge.source] + edge.weight;
                    parent[edge.destination] = edge.source;
                }
            }
        }

        for (Edge edge : edges) {
            if (distances[edge.source] != Integer.MAX_VALUE && distances[edge.destination] > distances[edge.source] + edge.weight) {
                System.out.println("Negative cycle detected.");
                return false;
            }
        }
        return true;
    }

    public void addEdge(int source, int destination, int weight) {
        edges[edgeIndex++] = new Edge(source, destination, weight);
        edgeCount = edgeIndex;
        vertexCount = calculateMaxVertex(edges) + 1;
    }

    public Edge[] getEdgeArray() {
        return Arrays.copyOf(edges, edgeIndex);
    }

    private int calculateMaxVertex(Edge[] edges) {
        int maxVertex = -1;
        if(edges == null || edges.length == 0) return maxVertex;
        for (Edge edge : edges) {
            maxVertex = Math.max(maxVertex, Math.max(edge.source, edge.destination));
        }
        return maxVertex;
    }
}