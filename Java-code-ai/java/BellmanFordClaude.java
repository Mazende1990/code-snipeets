package java;

import java.util.Scanner;

/**
 * Implementation of the Bellman-Ford algorithm to detect negative cycles and find shortest paths.
 * The graph accepts inputs in the form of edges with start vertex, end vertex, and weights.
 * Vertices should be labeled with numbers from 0 to (totalVertices - 1), inclusive.
 */
public class BellmanFordClaude {
    private int numVertices;
    private int numEdges;
    private Edge[] edges;
    private int edgeCount = 0;

    /**
     * Represents an edge in the graph with source, destination, and weight.
     */
    static class Edge {
        int source;
        int destination;
        int weight;

        /**
         * Constructs an edge with given source, destination, and weight.
         * 
         * @param source Source vertex
         * @param destination Destination vertex
         * @param weight Edge weight
         */
        public Edge(int source, int destination, int weight) {
            this.source = source;
            this.destination = destination;
            this.weight = weight;
        }
    }

    /**
     * Constructs a BellmanFord object with given number of vertices and edges.
     * 
     * @param vertices Number of vertices in the graph
     * @param edges Number of edges in the graph
     */
    public BellmanFordClaude(int vertices, int edges) {
        this.numVertices = vertices;
        this.numEdges = edges;
        this.edges = new Edge[edges];
    }

    /**
     * Adds a directed edge to the graph.
     * 
     * @param source Source vertex
     * @param destination Destination vertex
     * @param weight Edge weight
     */
    public void addEdge(int source, int destination, int weight) {
        if (edgeCount < numEdges) {
            edges[edgeCount++] = new Edge(source, destination, weight);
        } else {
            throw new IllegalStateException("Cannot add more edges than specified at initialization");
        }
    }

    /**
     * Returns the current edge array.
     * 
     * @return Array of edges
     */
    public Edge[] getEdgeArray() {
        return edges;
    }

    /**
     * Recursively prints the path from source to the given vertex.
     * 
     * @param parent Parent array that shows updates in edges
     * @param vertex Current vertex under consideration
     */
    private void printPath(int[] parent, int vertex) {
        if (parent[vertex] == -1) { // Found the path back to source
            return;
        }
        printPath(parent, parent[vertex]);
        System.out.print(vertex + " ");
    }

    /**
     * Runs the Bellman-Ford algorithm and shows the shortest path from source to destination.
     * 
     * @param source Source vertex
     * @param destination Destination vertex
     * @param edgeArray Array of edges in the graph
     */
    public void findShortestPath(int source, int destination, Edge[] edgeArray) {
        int[] distance = new int[numVertices];  // Holds shortest path distances
        int[] parent = new int[numVertices];    // Holds the paths
        boolean hasNegativeCycle = false;

        // Initialize distances
        for (int i = 0; i < numVertices; i++) {
            distance[i] = Integer.MAX_VALUE;
            parent[i] = -1;
        }
        distance[source] = 0;

        // Relax all edges |V|-1 times
        for (int i = 0; i < numVertices - 1; i++) {
            for (int j = 0; j < numEdges; j++) {
                Edge edge = edgeArray[j];
                if (distance[edge.source] != Integer.MAX_VALUE && 
                    distance[edge.destination] > distance[edge.source] + edge.weight) {
                    distance[edge.destination] = distance[edge.source] + edge.weight;
                    parent[edge.destination] = edge.source;
                }
            }
        }

        // Check for negative weight cycles
        for (int j = 0; j < numEdges; j++) {
            Edge edge = edgeArray[j];
            if (distance[edge.source] != Integer.MAX_VALUE && 
                distance[edge.destination] > distance[edge.source] + edge.weight) {
                hasNegativeCycle = true;
                System.out.println("Negative cycle detected");
                break;
            }
        }

        // Display results
        if (!hasNegativeCycle) {
            System.out.println("Distance is: " + distance[destination]);
            System.out.println("Path followed:");
            System.out.print(source + " ");
            printPath(parent, destination);
            System.out.println();
        }
    }

    /**
     * Runs the Bellman-Ford algorithm and shows shortest paths from source to all vertices.
     */
    public void findAllShortestPaths() {
        Scanner scanner = new Scanner(System.in);
        
        // Get graph information
        System.out.println("Enter number of vertices and edges:");
        int vertices = scanner.nextInt();
        int edgesCount = scanner.nextInt();
        Edge[] graphEdges = new Edge[edgesCount];
        
        // Get edge information
        System.out.println("Input edges (source destination weight):");
        for (int i = 0; i < edgesCount; i++) {
            int source = scanner.nextInt();
            int destination = scanner.nextInt();
            int weight = scanner.nextInt();
            graphEdges[i] = new Edge(source, destination, weight);
        }
        
        int[] distance = new int[vertices];
        int[] parent = new int[vertices];
        boolean hasNegativeCycle = false;
        
        // Initialize distances
        for (int i = 0; i < vertices; i++) {
            distance[i] = Integer.MAX_VALUE;
            parent[i] = -1;
        }
        distance[0] = 0;  // Source vertex is 0
        
        // Relax all edges |V|-1 times
        for (int i = 0; i < vertices - 1; i++) {
            for (int j = 0; j < edgesCount; j++) {
                Edge edge = graphEdges[j];
                if (distance[edge.source] != Integer.MAX_VALUE && 
                    distance[edge.destination] > distance[edge.source] + edge.weight) {
                    distance[edge.destination] = distance[edge.source] + edge.weight;
                    parent[edge.destination] = edge.source;
                }
            }
        }
        
        // Check for negative weight cycles
        for (int j = 0; j < edgesCount; j++) {
            Edge edge = graphEdges[j];
            if (distance[edge.source] != Integer.MAX_VALUE && 
                distance[edge.destination] > distance[edge.source] + edge.weight) {
                hasNegativeCycle = true;
                System.out.println("Negative cycle detected");
                break;
            }
        }
        
        // Display results
        if (!hasNegativeCycle) {
            System.out.println("Distances from vertex 0:");
            for (int i = 0; i < vertices; i++) {
                System.out.println(i + ": " + distance[i]);
            }
            
            System.out.println("Paths from vertex 0:");
            for (int i = 0; i < vertices; i++) {
                System.out.print("0 ");
                printPath(parent, i);
                System.out.println();
            }
        }
        
        scanner.close();
    }

    /**
     * Main method to demonstrate the algorithm.
     */
    public static void main(String[] args) {
        BellmanFordClaude algorithm = new BellmanFordClaude(0, 0);
        algorithm.findAllShortestPaths();
    }
}