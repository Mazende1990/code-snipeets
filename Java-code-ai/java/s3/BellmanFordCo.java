package java.s3;

import java.util.*;

class BellmanFordCo {

    private int vertex, edge;
    private Edge[] edges;
    private int index = 0;

    BellmanFordCo(int v, int e) {
        vertex = v;
        edge = e;
        edges = new Edge[e];
    }

    class Edge {
        int u, v, w;

        Edge(int u, int v, int w) {
            this.u = u;
            this.v = v;
            this.w = w;
        }
    }

    void printPath(int[] parent, int i) {
        if (parent[i] == -1) {
            return;
        }
        printPath(parent, parent[i]);
        System.out.print(i + " ");
    }

    public static void main(String[] args) {
        BellmanFordCo obj = new BellmanFordCo(0, 0);
        obj.runInteractive();
    }

    public void runInteractive() {
        Scanner sc = new Scanner(System.in);
        System.out.println("Enter number of vertices and edges:");
        int v = sc.nextInt();
        int e = sc.nextInt();
        Edge[] arr = new Edge[e];

        System.out.println("Input edges (u v w):");
        for (int i = 0; i < e; i++) {
            int u = sc.nextInt();
            int ve = sc.nextInt();
            int w = sc.nextInt();
            arr[i] = new Edge(u, ve, w);
        }

        int[] dist = new int[v];
        int[] parent = new int[v];
        Arrays.fill(dist, Integer.MAX_VALUE);
        dist[0] = 0;
        parent[0] = -1;

        for (int i = 0; i < v - 1; i++) {
            for (Edge edge : arr) {
                if (dist[edge.u] != Integer.MAX_VALUE && dist[edge.v] > dist[edge.u] + edge.w) {
                    dist[edge.v] = dist[edge.u] + edge.w;
                    parent[edge.v] = edge.u;
                }
            }
        }

        boolean hasNegativeCycle = false;
        for (Edge edge : arr) {
            if (dist[edge.u] != Integer.MAX_VALUE && dist[edge.v] > dist[edge.u] + edge.w) {
                hasNegativeCycle = true;
                System.out.println("Negative cycle detected");
                break;
            }
        }

        if (!hasNegativeCycle) {
            System.out.println("Distances:");
            for (int i = 0; i < v; i++) {
                System.out.println(i + " " + dist[i]);
            }
            System.out.println("Paths:");
            for (int i = 0; i < v; i++) {
                System.out.print("0 ");
                printPath(parent, i);
                System.out.println();
            }
        }
        sc.close();
    }

    public void show(int source, int end, Edge[] arr) {
        int[] dist = new int[vertex];
        int[] parent = new int[vertex];
        Arrays.fill(dist, Integer.MAX_VALUE);
        dist[source] = 0;
        parent[source] = -1;

        for (int i = 0; i < vertex - 1; i++) {
            for (Edge edge : arr) {
                if (dist[edge.u] != Integer.MAX_VALUE && dist[edge.v] > dist[edge.u] + edge.w) {
                    dist[edge.v] = dist[edge.u] + edge.w;
                    parent[edge.v] = edge.u;
                }
            }
        }

        boolean hasNegativeCycle = false;
        for (Edge edge : arr) {
            if (dist[edge.u] != Integer.MAX_VALUE && dist[edge.v] > dist[edge.u] + edge.w) {
                hasNegativeCycle = true;
                System.out.println("Negative cycle detected");
                break;
            }
        }

        if (!hasNegativeCycle) {
            System.out.println("Distance to " + end + ": " + dist[end]);
            System.out.println("Path:");
            System.out.print(source + " ");
            printPath(parent, end);
            System.out.println();
        }
    }

    public void addEdge(int x, int y, int z) {
        edges[index++] = new Edge(x, y, z);
    }

    public Edge[] getEdgeArray() {
        return edges;
    }
}