package java.s4;

import java.util.*;

/**
 * Graph. Could be directed or undirected depending on the TYPE enum.
 * Represents a set of vertices and edges with optional direction and cost.
 */
@SuppressWarnings("unchecked")
public class Graph<T extends Comparable<T>> {

    public enum TYPE { DIRECTED, UNDIRECTED }

    private final List<Vertex<T>> allVertices = new ArrayList<>();
    private final List<Edge<T>> allEdges = new ArrayList<>();
    private TYPE type = TYPE.UNDIRECTED;

    public Graph() {}

    public Graph(TYPE type) {
        this.type = type;
    }

    public Graph(Graph<T> g) {
        this.type = g.getType();
        for (Vertex<T> v : g.getVertices()) this.allVertices.add(new Vertex<>(v));
        for (Vertex<T> v : this.getVertices()) this.allEdges.addAll(v.getEdges());
    }

    public Graph(Collection<Vertex<T>> vertices, Collection<Edge<T>> edges) {
        this(TYPE.UNDIRECTED, vertices, edges);
    }

    public Graph(TYPE type, Collection<Vertex<T>> vertices, Collection<Edge<T>> edges) {
        this.type = type;
        this.allVertices.addAll(vertices);
        this.allEdges.addAll(edges);

        for (Edge<T> e : edges) {
            Vertex<T> from = e.from;
            Vertex<T> to = e.to;
            if (!allVertices.contains(from) || !allVertices.contains(to)) continue;

            from.addEdge(e);
            if (type == TYPE.UNDIRECTED) {
                Edge<T> reciprocal = new Edge<>(e.cost, to, from);
                to.addEdge(reciprocal);
                allEdges.add(reciprocal);
            }
        }
    }

    public TYPE getType() { return type; }
    public List<Vertex<T>> getVertices() { return allVertices; }
    public List<Edge<T>> getEdges() { return allEdges; }

    @Override
    public int hashCode() {
        return 31 * (type.hashCode() + allVertices.hashCode() + allEdges.hashCode());
    }

    @Override
    public boolean equals(Object obj) {
        if (!(obj instanceof Graph)) return false;
        Graph<T> other = (Graph<T>) obj;
        return type == other.type && new HashSet<>(allVertices).equals(new HashSet<>(other.allVertices))
                && new HashSet<>(allEdges).equals(new HashSet<>(other.allEdges));
    }

    @Override
    public String toString() {
        StringBuilder sb = new StringBuilder();
        for (Vertex<T> v : allVertices) sb.append(v);
        return sb.toString();
    }

    public static class Vertex<T extends Comparable<T>> implements Comparable<Vertex<T>> {
        private final T value;
        private int weight;
        private final List<Edge<T>> edges = new ArrayList<>();

        public Vertex(T value) { this.value = value; }
        public Vertex(T value, int weight) { this(value); this.weight = weight; }
        public Vertex(Vertex<T> vertex) { this(vertex.value, vertex.weight); edges.addAll(vertex.edges); }

        public T getValue() { return value; }
        public int getWeight() { return weight; }
        public void setWeight(int weight) { this.weight = weight; }
        public void addEdge(Edge<T> e) { edges.add(e); }
        public List<Edge<T>> getEdges() { return edges; }

        public Edge<T> getEdge(Vertex<T> v) {
            return edges.stream().filter(e -> e.to.equals(v)).findFirst().orElse(null);
        }

        public boolean pathTo(Vertex<T> v) {
            return edges.stream().anyMatch(e -> e.to.equals(v));
        }

        @Override
        public int hashCode() {
            return 31 * Objects.hash(value, weight, edges);
        }

        @Override
        public boolean equals(Object obj) {
            if (!(obj instanceof Vertex)) return false;
            Vertex<T> other = (Vertex<T>) obj;
            return value.equals(other.value) && weight == other.weight && edges.size() == other.edges.size();
        }

        @Override
        public int compareTo(Vertex<T> o) {
            int cmp = value.compareTo(o.value);
            if (cmp != 0) return cmp;
            cmp = Integer.compare(weight, o.weight);
            if (cmp != 0) return cmp;
            return Integer.compare(edges.size(), o.edges.size());
        }

        @Override
        public String toString() {
            StringBuilder sb = new StringBuilder();
            sb.append("Value=").append(value).append(" weight=").append(weight).append("\n");
            for (Edge<T> e : edges) sb.append("\t").append(e);
            return sb.toString();
        }
    }

    public static class Edge<T extends Comparable<T>> implements Comparable<Edge<T>> {
        private final Vertex<T> from;
        private final Vertex<T> to;
        private int cost;

        public Edge(int cost, Vertex<T> from, Vertex<T> to) {
            if (from == null || to == null) throw new NullPointerException("Both vertices must be non-null.");
            this.cost = cost; this.from = from; this.to = to;
        }

        public Edge(Edge<T> e) { this(e.cost, e.from, e.to); }

        public int getCost() { return cost; }
        public void setCost(int cost) { this.cost = cost; }
        public Vertex<T> getFromVertex() { return from; }
        public Vertex<T> getToVertex() { return to; }

        @Override
        public int hashCode() {
            return 31 * Objects.hash(cost, from, to);
        }

        @Override
        public boolean equals(Object obj) {
            if (!(obj instanceof Edge)) return false;
            Edge<T> other = (Edge<T>) obj;
            return cost == other.cost && from.equals(other.from) && to.equals(other.to);
        }

        @Override
        public int compareTo(Edge<T> o) {
            int cmp = Integer.compare(cost, o.cost);
            if (cmp != 0) return cmp;
            cmp = from.compareTo(o.from);
            if (cmp != 0) return cmp;
            return to.compareTo(o.to);
        }

        @Override
        public String toString() {
            return String.format("[ %s(%d) ] -> [ %s(%d) ] = %d\n",
                    from.value, from.weight, to.value, to.weight, cost);
        }
    }

    public static class CostVertexPair<T extends Comparable<T>> implements Comparable<CostVertexPair<T>> {
        private int cost;
        private final Vertex<T> vertex;

        public CostVertexPair(int cost, Vertex<T> vertex) {
            if (vertex == null) throw new NullPointerException("vertex cannot be NULL.");
            this.cost = cost;
            this.vertex = vertex;
        }

        public int getCost() { return cost; }
        public void setCost(int cost) { this.cost = cost; }
        public Vertex<T> getVertex() { return vertex; }

        @Override
        public int hashCode() { return 31 * Objects.hash(cost, vertex); }

        @Override
        public boolean equals(Object obj) {
            if (!(obj instanceof CostVertexPair)) return false;
            CostVertexPair<?> other = (CostVertexPair<?>) obj;
            return cost == other.cost && vertex.equals(other.vertex);
        }

        @Override
        public int compareTo(CostVertexPair<T> o) {
            return Integer.compare(cost, o.cost);
        }

        @Override
        public String toString() {
            return String.format("%s (%d) cost=%d\n", vertex.getValue(), vertex.getWeight(), cost);
        }
    }

    public static class CostPathPair<T extends Comparable<T>> {
        private int cost;
        private final List<Edge<T>> path;

        public CostPathPair(int cost, List<Edge<T>> path) {
            if (path == null) throw new NullPointerException("path cannot be NULL.");
            this.cost = cost;
            this.path = path;
        }

        public int getCost() { return cost; }
        public void setCost(int cost) { this.cost = cost; }
        public List<Edge<T>> getPath() { return path; }

        @Override
        public int hashCode() {
            return 31 * Objects.hash(cost, path);
        }

        @Override
        public boolean equals(Object obj) {
            if (!(obj instanceof CostPathPair)) return false;
            CostPathPair<?> other = (CostPathPair<?>) obj;
            return cost == other.cost && path.equals(other.path);
        }

        @Override
        public String toString() {
            StringBuilder sb = new StringBuilder("Cost = ").append(cost).append("\n");
            for (Edge<T> e : path) sb.append("\t").append(e);
            return sb.toString();
        }
    }
}
