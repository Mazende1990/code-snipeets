package java;

import java.util.Scanner;

class BellmanFordDeep {

    private int vertexCount;
    private int edgeCount;
    private Edge[] edges;
    private int edgeIndex = 0;

    BellmanFordDeep(int vertexCount, int edgeCount) {
        this.vertexCount = vertexCount;
        this.edgeCount = edgeCount;
        this.edges = new Edge[edgeCount];
    }

    class Edge {
        int startVertex;
        int endVertex;
        int weight;

        public Edge(int startVertex, int endVertex, int weight) {
            this.startVertex = startVertex;
            this.endVertex = endVertex;
            this.weight = weight;
        }
    }

    public static void main(String[] args) {
        BellmanFordDeep bellmanFord = new BellmanFordDeep(0, 0); // Dummy object to call non-static methods
        bellmanFord.runInteractive();
    }

    public void runInteractive() {
        Scanner scanner = new Scanner(System.in);
        System.out.println("Enter the number of vertices and edges:");
        int vertexCount = scanner.nextInt();
        int edgeCount = scanner.nextInt();

        Edge[] edges = new Edge[edgeCount];
        System.out.println("Input edges (startVertex, endVertex, weight):");
        for (int i = 0; i < edgeCount; i++) {
            int startVertex = scanner.nextInt();
            int endVertex = scanner.nextInt();
            int weight = scanner.nextInt();
            edges[i] = new Edge(startVertex, endVertex, weight);
        }

        int[] distances = new int[vertexCount];
        int[] predecessors = new int[vertexCount];
        initializeSingleSource(distances, predecessors, 0);

        boolean hasNegativeCycle = relaxEdges(edges, distances, predecessors);

        if (!hasNegativeCycle) {
            printDistances(distances);
            printPaths(predecessors);
        } else {
            System.out.println("Negative cycle detected.");
        }

        scanner.close();
    }

    private void initializeSingleSource(int[] distances, int[] predecessors, int source) {
        for (int i = 0; i < vertexCount; i++) {
            distances[i] = Integer.MAX_VALUE;
        }
        distances[source] = 0;
        predecessors[source] = -1;
    }

    private boolean relaxEdges(Edge[] edges, int[] distances, int[] predecessors) {
        for (int i = 0; i < vertexCount - 1; i++) {
            for (Edge edge : edges) {
                if (distances[edge.startVertex] != Integer.MAX_VALUE && 
                    distances[edge.endVertex] > distances[edge.startVertex] + edge.weight) {
                    distances[edge.endVertex] = distances[edge.startVertex] + edge.weight;
                    predecessors[edge.endVertex] = edge.startVertex;
                }
            }
        }

        for (Edge edge : edges) {
            if (distances[edge.startVertex] != Integer.MAX_VALUE && 
                distances[edge.endVertex] > distances[edge.startVertex] + edge.weight) {
                return true; // Negative cycle detected
            }
        }
        return false;
    }

    private void printDistances(int[] distances) {
        System.out.println("Distances from source:");
        for (int i = 0; i < vertexCount; i++) {
            System.out.println("Vertex " + i + ": " + distances[i]);
        }
    }

    private void printPaths(int[] predecessors) {
        System.out.println("Paths from source:");
        for (int i = 0; i < vertexCount; i++) {
            System.out.print("Path to " + i + ": ");
            printPath(predecessors, i);
            System.out.println();
        }
    }

    private void printPath(int[] predecessors, int vertex) {
        if (predecessors[vertex] == -1) {
            System.out.print(vertex + " ");
            return;
        }
        printPath(predecessors, predecessors[vertex]);
        System.out.print(vertex + " ");
    }

    public void addEdge(int startVertex, int endVertex, int weight) {
        edges[edgeIndex++] = new Edge(startVertex, endVertex, weight);
    }

    public Edge[] getEdges() {
        return edges;
    }
}