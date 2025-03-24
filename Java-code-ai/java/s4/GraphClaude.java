package java.s4;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collection;
import java.util.Iterator;
import java.util.List;

/**
 * Graph implementation that supports both directed and undirected graphs.
 * <p>
 * A graph is an abstract representation of a set of objects (vertices) where some 
 * pairs of the objects are connected by links (edges).
 * <p>
 * @see <a href="https://en.wikipedia.org/wiki/Graph_(mathematics)">Graph (Wikipedia)</a>
 * <br>
 * @author Justin Wetherell <phishman3579@gmail.com>
 */
@SuppressWarnings("unchecked")
public class GraphClaude<T extends Comparable<T>> {

    /** Enum representing graph types */
    public enum GraphType {
        DIRECTED, UNDIRECTED
    }

    // Core graph data
    private final List<Vertex<T>> vertices = new ArrayList<>();
    private final List<Edge<T>> edges = new ArrayList<>();
    private GraphType graphType = GraphType.UNDIRECTED; // Default to undirected

    /**
     * Default constructor - creates an undirected graph
     */
    public GraphClaude() {
        // Default to undirected graph
    }

    /**
     * Creates a graph of the specified type
     * 
     * @param type The type of graph (DIRECTED or UNDIRECTED)
     */
    public GraphClaude(GraphType type) {
        this.graphType = type;
    }

    /**
     * Copy constructor - performs a deep copy of the provided graph
     * 
     * @param sourceGraph The graph to copy
     */
    public GraphClaude(GraphClaude<T> sourceGraph) {
        this.graphType = sourceGraph.getType();

        // Copy the vertices which also copies the edges
        for (Vertex<T> vertex : sourceGraph.getVertices()) {
            this.vertices.add(new Vertex<>(vertex));
        }

        // Add all edges to the edge list
        for (Vertex<T> vertex : this.getVertices()) {
            for (Edge<T> edge : vertex.getEdges()) {
                this.edges.add(edge);
            }
        }
    }

    /**
     * Creates an undirected graph from the provided vertices and edges
     * 
     * NOTE: Duplicate vertices and edges ARE allowed.
     * NOTE: Copies the vertex and edge objects but does NOT store the Collection parameters.
     * 
     * @param vertices Collection of vertices
     * @param edges Collection of edges
     */
    public GraphClaude(Collection<Vertex<T>> vertices, Collection<Edge<T>> edges) {
        this(GraphType.UNDIRECTED, vertices, edges);
    }

    /**
     * Creates a graph of the specified type from the provided vertices and edges
     * 
     * NOTE: Duplicate vertices and edges ARE allowed.
     * NOTE: Copies the vertex and edge objects but does NOT store the Collection parameters.
     * 
     * @param type Type of graph (DIRECTED or UNDIRECTED)
     * @param vertices Collection of vertices
     * @param edges Collection of edges
     */
    public GraphClaude(GraphType type, Collection<Vertex<T>> vertices, Collection<Edge<T>> edges) {
        this(type);

        // Add all vertices and edges to the graph
        this.vertices.addAll(vertices);
        this.edges.addAll(edges);

        // Connect vertices with edges
        connectVerticesWithEdges(edges);
    }

    /**
     * Helper method to connect vertices with the provided edges
     */
    private void connectVerticesWithEdges(Collection<Edge<T>> edgeCollection) {
        for (Edge<T> edge : edgeCollection) {
            final Vertex<T> fromVertex = edge.from;
            final Vertex<T> toVertex = edge.to;

            // Skip edges that reference vertices not in this graph
            if (!this.vertices.contains(fromVertex) || !this.vertices.contains(toVertex)) {
                continue;
            }

            // Add edge to "from" vertex
            fromVertex.addEdge(edge);

            // For undirected graphs, add reciprocal edge
            if (this.graphType == GraphType.UNDIRECTED) {
                Edge<T> reciprocalEdge = new Edge<>(edge.cost, toVertex, fromVertex);
                toVertex.addEdge(reciprocalEdge);
                this.edges.add(reciprocalEdge);
            }
        }
    }

    /**
     * @return The type of this graph (DIRECTED or UNDIRECTED)
     */
    public GraphType getType() {
        return graphType;
    }

    /**
     * @return List of all vertices in the graph
     */
    public List<Vertex<T>> getVertices() {
        return vertices;
    }

    /**
     * @return List of all edges in the graph
     */
    public List<Edge<T>> getEdges() {
        return edges;
    }

    /**
     * {@inheritDoc}
     */
    @Override
    public int hashCode() {
        int code = this.graphType.hashCode() + this.vertices.size() + this.edges.size();
        
        for (Vertex<T> vertex : vertices) {
            code *= vertex.hashCode();
        }
        
        for (Edge<T> edge : edges) {
            code *= edge.hashCode();
        }
        
        return 31 * code;
    }

    /**
     * {@inheritDoc}
     */
    @Override
    public boolean equals(Object obj) {
        if (!(obj instanceof GraphClaude)) {
            return false;
        }

        final GraphClaude<T> otherGraph = (GraphClaude<T>) obj;

        // Check basic properties first
        if (this.graphType != otherGraph.graphType ||
            this.vertices.size() != otherGraph.vertices.size() ||
            this.edges.size() != otherGraph.edges.size()) {
            return false;
        }

        // Compare sorted vertices
        if (!compareVertexArrays(this.vertices, otherGraph.vertices)) {
            return false;
        }

        // Compare sorted edges
        if (!compareEdgeArrays(this.edges, otherGraph.edges)) {
            return false;
        }

        return true;
    }
    
    /**
     * Helper method to compare two vertex collections
     */
    private boolean compareVertexArrays(List<Vertex<T>> vertices1, List<Vertex<T>> vertices2) {
        // Vertices can contain duplicates and appear in different order
        // but both arrays should contain the same elements
        final Object[] vertexArray1 = vertices1.toArray();
        final Object[] vertexArray2 = vertices2.toArray();
        
        Arrays.sort(vertexArray1);
        Arrays.sort(vertexArray2);
        
        for (int i = 0; i < vertexArray1.length; i++) {
            final Vertex<T> v1 = (Vertex<T>) vertexArray1[i];
            final Vertex<T> v2 = (Vertex<T>) vertexArray2[i];
            if (!v1.equals(v2)) {
                return false;
            }
        }
        
        return true;
    }
    
    /**
     * Helper method to compare two edge collections
     */
    private boolean compareEdgeArrays(List<Edge<T>> edges1, List<Edge<T>> edges2) {
        // Edges can contain duplicates and appear in different order
        // but both arrays should contain the same elements
        final Object[] edgeArray1 = edges1.toArray();
        final Object[] edgeArray2 = edges2.toArray();
        
        Arrays.sort(edgeArray1);
        Arrays.sort(edgeArray2);
        
        for (int i = 0; i < edgeArray1.length; i++) {
            final Edge<T> e1 = (Edge<T>) edgeArray1[i];
            final Edge<T> e2 = (Edge<T>) edgeArray2[i];
            if (!e1.equals(e2)) {
                return false;
            }
        }
        
        return true;
    }

    /**
     * {@inheritDoc}
     */
    @Override
    public String toString() {
        final StringBuilder builder = new StringBuilder();
        for (Vertex<T> vertex : vertices) {
            builder.append(vertex.toString());
        }
        return builder.toString();
    }

    /**
     * Vertex in a graph with a value, weight, and connected edges.
     */
    public static class Vertex<T extends Comparable<T>> implements Comparable<Vertex<T>> {
        // Core vertex data
        private final T value;
        private int weight;
        private final List<Edge<T>> edges = new ArrayList<>();

        /**
         * Creates a vertex with the given value and default weight of 0
         * 
         * @param value The value of this vertex
         */
        public Vertex(T value) {
            this.value = value;
            this.weight = 0;
        }

        /**
         * Creates a vertex with the given value and weight
         * 
         * @param value The value of this vertex
         * @param weight The weight of this vertex
         */
        public Vertex(T value, int weight) {
            this.value = value;
            this.weight = weight;
        }

        /**
         * Copy constructor - performs a deep copy of the provided vertex
         * 
         * @param vertex The vertex to copy
         */
        public Vertex(Vertex<T> vertex) {
            this.value = vertex.value;
            this.weight = vertex.weight;
            this.edges.addAll(vertex.edges);
        }

        /**
         * @return The value of this vertex
         */
        public T getValue() {
            return value;
        }

        /**
         * @return The weight of this vertex
         */
        public int getWeight() {
            return weight;
        }

        /**
         * Sets the weight of this vertex
         * 
         * @param weight The new weight
         */
        public void setWeight(int weight) {
            this.weight = weight;
        }

        /**
         * Adds an edge to this vertex
         * 
         * @param edge The edge to add
         */
        public void addEdge(Edge<T> edge) {
            edges.add(edge);
        }

        /**
         * @return List of all edges connected to this vertex
         */
        public List<Edge<T>> getEdges() {
            return edges;
        }

        /**
         * Finds an edge from this vertex to the specified vertex
         * 
         * @param targetVertex The target vertex
         * @return The edge to the target vertex, or null if no such edge exists
         */
        public Edge<T> getEdge(Vertex<T> targetVertex) {
            for (Edge<T> edge : edges) {
                if (edge.to.equals(targetVertex)) {
                    return edge;
                }
            }
            return null;
        }

        /**
         * Checks if there is a path from this vertex to the specified vertex
         * 
         * @param targetVertex The target vertex
         * @return True if a direct edge exists to the target vertex
         */
        public boolean pathTo(Vertex<T> targetVertex) {
            for (Edge<T> edge : edges) {
                if (edge.to.equals(targetVertex)) {
                    return true;
                }
            }
            return false;
        }

        /**
         * {@inheritDoc}
         */
        @Override
        public int hashCode() {
            final int code = this.value.hashCode() + this.weight + this.edges.size();
            return 31 * code;
        }

        /**
         * {@inheritDoc}
         */
        @Override
        public boolean equals(Object obj) {
            if (!(obj instanceof Vertex)) {
                return false;
            }

            final Vertex<T> otherVertex = (Vertex<T>) obj;

            // Check value, weight, and edges size
            if (!this.value.equals(otherVertex.value) || 
                this.weight != otherVertex.weight || 
                this.edges.size() != otherVertex.edges.size()) {
                return false;
            }

            // Compare edge costs
            final Iterator<Edge<T>> thisEdges = this.edges.iterator();
            final Iterator<Edge<T>> otherEdges = otherVertex.edges.iterator();
            
            while (thisEdges.hasNext() && otherEdges.hasNext()) {
                // Only checking the cost
                final Edge<T> thisEdge = thisEdges.next();
                final Edge<T> otherEdge = otherEdges.next();
                
                if (thisEdge.cost != otherEdge.cost) {
                    return false;
                }
            }

            return true;
        }

        /**
         * {@inheritDoc}
         */
        @Override
        public int compareTo(Vertex<T> otherVertex) {
            // Compare by value first
            final int valueComparison = this.value.compareTo(otherVertex.value);
            if (valueComparison != 0) {
                return valueComparison;
            }

            // Then by weight
            if (this.weight < otherVertex.weight) return -1;
            if (this.weight > otherVertex.weight) return 1;

            // Then by number of edges
            if (this.edges.size() < otherVertex.edges.size()) return -1;
            if (this.edges.size() > otherVertex.edges.size()) return 1;

            // Finally, compare edge costs
            final Iterator<Edge<T>> thisEdges = this.edges.iterator();
            final Iterator<Edge<T>> otherEdges = otherVertex.edges.iterator();
            
            while (thisEdges.hasNext() && otherEdges.hasNext()) {
                final Edge<T> thisEdge = thisEdges.next();
                final Edge<T> otherEdge = otherEdges.next();
                
                if (thisEdge.cost < otherEdge.cost) return -1;
                if (thisEdge.cost > otherEdge.cost) return 1;
            }

            return 0;
        }

        /**
         * {@inheritDoc}
         */
        @Override
        public String toString() {
            final StringBuilder builder = new StringBuilder();
            builder.append("Value=").append(value).append(" weight=").append(weight).append("\n");
            
            for (Edge<T> edge : edges) {
                builder.append("\t").append(edge.toString());
            }
            
            return builder.toString();
        }
    }

    /**
     * Edge in a graph connecting two vertices with an associated cost.
     */
    public static class Edge<T extends Comparable<T>> implements Comparable<Edge<T>> {
        // Core edge data
        private final Vertex<T> from;
        private final Vertex<T> to;
        private int cost;

        /**
         * Creates an edge from one vertex to another with the specified cost
         * 
         * @param cost The cost of traversing this edge
         * @param from The source vertex
         * @param to The destination vertex
         * @throws NullPointerException if either vertex is null
         */
        public Edge(int cost, Vertex<T> from, Vertex<T> to) {
            if (from == null || to == null) {
                throw new NullPointerException("Both 'to' and 'from' vertices must be non-NULL.");
            }

            this.cost = cost;
            this.from = from;
            this.to = to;
        }

        /**
         * Copy constructor
         * 
         * @param edge The edge to copy
         */
        public Edge(Edge<T> edge) {
            this(edge.cost, edge.from, edge.to);
        }

        /**
         * @return The cost of this edge
         */
        public int getCost() {
            return cost;
        }

        /**
         * Sets the cost of this edge
         * 
         * @param cost The new cost
         */
        public void setCost(int cost) {
            this.cost = cost;
        }

        /**
         * @return The source vertex
         */
        public Vertex<T> getFromVertex() {
            return from;
        }

        /**
         * @return The destination vertex
         */
        public Vertex<T> getToVertex() {
            return to;
        }

        /**
         * {@inheritDoc}
         */
        @Override
        public int hashCode() {
            final int hashValue = (this.cost * (this.getFromVertex().hashCode() * this.getToVertex().hashCode())); 
            return 31 * hashValue;
        }

        /**
         * {@inheritDoc}
         */
        @Override
        public boolean equals(Object obj) {
            if (!(obj instanceof Edge)) {
                return false;
            }

            final Edge<T> otherEdge = (Edge<T>) obj;

            // Compare cost and vertices
            return this.cost == otherEdge.cost &&
                   this.from.equals(otherEdge.from) &&
                   this.to.equals(otherEdge.to);
        }

        /**
         * {@inheritDoc}
         */
        @Override
        public int compareTo(Edge<T> otherEdge) {
            // Compare by cost first
            if (this.cost < otherEdge.cost) return -1;
            if (this.cost > otherEdge.cost) return 1;

            // Then by source vertex
            final int fromComparison = this.from.compareTo(otherEdge.from);
            if (fromComparison != 0) {
                return fromComparison;
            }

            // Then by destination vertex
            return this.to.compareTo(otherEdge.to);
        }

        /**
         * {@inheritDoc}
         */
        @Override
        public String toString() {
            StringBuilder builder = new StringBuilder();
            builder.append("[ ")
                   .append(from.value).append("(").append(from.weight).append(") ")
                   .append("]").append(" -> ")
                   .append("[ ")
                   .append(to.value).append("(").append(to.weight).append(") ")
                   .append("]").append(" = ").append(cost).append("\n");
            return builder.toString();
        }
    }

    /**
     * Helper class to associate a cost with a vertex.
     * Used in algorithms like Dijkstra's shortest path.
     */
    public static class CostVertexPair<T extends Comparable<T>> implements Comparable<CostVertexPair<T>> {
        private int cost;
        private final Vertex<T> vertex;

        /**
         * Creates a cost-vertex pair
         * 
         * @param cost The cost to reach this vertex
         * @param vertex The vertex
         * @throws NullPointerException if vertex is null
         */
        public CostVertexPair(int cost, Vertex<T> vertex) {
            if (vertex == null) {
                throw new NullPointerException("Vertex cannot be NULL.");
            }

            this.cost = cost;
            this.vertex = vertex;
        }

        /**
         * @return The cost to reach this vertex
         */
        public int getCost() {
            return cost;
        }

        /**
         * Sets the cost to reach this vertex
         * 
         * @param cost The new cost
         */
        public void setCost(int cost) {
            this.cost = cost;
        }

        /**
         * @return The vertex
         */
        public Vertex<T> getVertex() {
            return vertex;
        }

        /**
         * {@inheritDoc}
         */
        @Override
        public int hashCode() {
            return 31 * (this.cost * this.vertex.hashCode());
        }

        /**
         * {@inheritDoc}
         */
        @Override
        public boolean equals(Object obj) {
            if (!(obj instanceof CostVertexPair)) {
                return false;
            }

            final CostVertexPair<?> otherPair = (CostVertexPair<?>) obj;
            
            return this.cost == otherPair.cost && 
                   this.vertex.equals(otherPair.vertex);
        }

        /**
         * {@inheritDoc}
         */
        @Override
        public int compareTo(CostVertexPair<T> otherPair) {
            if (otherPair == null) {
                throw new NullPointerException("CostVertexPair to compare must be non-NULL.");
            }

            // Compare by cost
            if (this.cost < otherPair.cost) return -1;
            if (this.cost > otherPair.cost) return 1;
            
            return 0;
        }

        /**
         * {@inheritDoc}
         */
        @Override
        public String toString() {
            final StringBuilder builder = new StringBuilder();
            builder.append(vertex.getValue())
                   .append(" (").append(vertex.weight).append(") ")
                   .append(" cost=").append(cost).append("\n");
            return builder.toString();
        }
    }

    /**
     * Helper class to associate a cost with a path (sequence of edges).
     * Used in path-finding algorithms.
     */
    public static class CostPathPair<T extends Comparable<T>> {
        private int cost;
        private final List<Edge<T>> path;

        /**
         * Creates a cost-path pair
         * 
         * @param cost The total cost of the path
         * @param path The path as a list of edges
         * @throws NullPointerException if path is null
         */
        public CostPathPair(int cost, List<Edge<T>> path) {
            if (path == null) {
                throw new NullPointerException("Path cannot be NULL.");
            }

            this.cost = cost;
            this.path = path;
        }

        /**
         * @return The total cost of the path
         */
        public int getCost() {
            return cost;
        }

        /**
         * Sets the total cost of the path
         * 
         * @param cost The new cost
         */
        public void setCost(int cost) {
            this.cost = cost;
        }

        /**
         * @return The path as a list of edges
         */
        public List<Edge<T>> getPath() {
            return path;
        }

        /**
         * {@inheritDoc}
         */
        @Override
        public int hashCode() {
            int hash = this.cost;
            for (Edge<T> edge : path) {
                hash *= edge.cost;
            }
            return 31 * hash;
        }

        /**
         * {@inheritDoc}
         */
        @Override
        public boolean equals(Object obj) {
            if (!(obj instanceof CostPathPair)) {
                return false;
            }

            final CostPathPair<?> otherPair = (CostPathPair<?>) obj;
            
            // Compare cost
            if (this.cost != otherPair.cost) {
                return false;
            }

            // Compare paths
            final Iterator<?> thisPath = this.getPath().iterator();
            final Iterator<?> otherPath = otherPair.getPath().iterator();
            
            while (thisPath.hasNext() && otherPath.hasNext()) {
                Edge<T> thisEdge = (Edge<T>) thisPath.next();
                Edge<T> otherEdge = (Edge<T>) otherPath.next();
                
                if (!thisEdge.equals(otherEdge)) {
                    return false;
                }
            }

            return true;
        }

        /**
         * {@inheritDoc}
         */
        @Override
        public String toString() {
            final StringBuilder builder = new StringBuilder();
            builder.append("Cost = ").append(cost).append("\n");
            
            for (Edge<T> edge : path) {
                builder.append("\t").append(edge);
            }
            
            return builder.toString();
        }
    }
}