package java.s4;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collection;
import java.util.Iterator;
import java.util.List;
import java.util.Objects;

/**
 * GraphGemini. Could be directed or undirected depending on the TYPE enum. A GraphGemini is
 * an abstract representation of a set of objects where some pairs of the
 * objects are connected by links.
 * <p>
 * @see <a href="https://en.wikipedia.org/wiki/GraphGemini_(mathematics)">GraphGemini (Wikipedia)</a>
 * <br>
 * @author Justin Wetherell <phishman3579@gmail.com>
 */
public class GraphGemini<T extends Comparable<T>> {

    private final List<Vertex<T>> vertices = new ArrayList<>();
    private final List<Edge<T>> edges = new ArrayList<>();
    private final TYPE type;

    public enum TYPE {
        DIRECTED, UNDIRECTED
    }

    public GraphGemini() {
        this(TYPE.UNDIRECTED);
    }

    public GraphGemini(TYPE type) {
        this.type = type;
    }

    /**
     * Deep copies.
     */
    public GraphGemini(GraphGemini<T> GraphGemini) {
        this(GraphGemini.getType());
        for (Vertex<T> vertex : GraphGemini.getVertices()) {
            this.vertices.add(new Vertex<>(vertex));
        }
        for (Vertex<T> vertex : this.getVertices()) {
            for (Edge<T> edge : vertex.getEdges()) {
                this.edges.add(edge);
            }
        }
    }

    /**
     * Creates a GraphGemini from the vertices and edges. Defaults to an undirected GraphGemini.
     * <p>
     * NOTE: Duplicate vertices and edges ARE allowed.
     * NOTE: Copies the vertex and edge objects but does NOT store the Collection parameters itself.
     *
     * @param vertices Collection of vertices
     * @param edges    Collection of edges
     */
    public GraphGemini(Collection<Vertex<T>> vertices, Collection<Edge<T>> edges) {
        this(TYPE.UNDIRECTED, vertices, edges);
    }

    /**
     * Creates a GraphGemini from the vertices and edges.
     * <p>
     * NOTE: Duplicate vertices and edges ARE allowed.
     * NOTE: Copies the vertex and edge objects but does NOT store the Collection parameters itself.
     *
     * @param vertices Collection of vertices
     * @param edges    Collection of edges
     */
    public GraphGemini(TYPE type, Collection<Vertex<T>> vertices, Collection<Edge<T>> edges) {
        this(type);
        this.vertices.addAll(vertices);
        this.edges.addAll(edges);
        for (Edge<T> edge : edges) {
            Vertex<T> from = edge.getFromVertex();
            Vertex<T> to = edge.getToVertex();
            if (this.vertices.contains(from) && this.vertices.contains(to)) {
                from.addEdge(edge);
                if (this.type == TYPE.UNDIRECTED) {
                    Edge<T> reciprocal = new Edge<>(edge.getCost(), to, from);
                    to.addEdge(reciprocal);
                    this.edges.add(reciprocal);
                }
            }
        }
    }

    public TYPE getType() {
        return type;
    }

    public List<Vertex<T>> getVertices() {
        return new ArrayList<>(vertices);
    }

    public List<Edge<T>> getEdges() {
        return new ArrayList<>(edges);
    }

    @Override
    public int hashCode() {
        return Objects.hash(type, vertices, edges);
    }

    @Override
    public boolean equals(Object obj) {
        if (this == obj) return true;
        if (obj == null || getClass() != obj.getClass()) return false;
        GraphGemini<?> other = (GraphGemini<?>) obj;
        if (this.type != other.type || this.vertices.size() != other.vertices.size() || this.edges.size() != other.edges.size()) {
            return false;
        }

        Object[] sortedVertices1 = this.vertices.toArray();
        Arrays.sort(sortedVertices1);
        Object[] sortedVertices2 = other.vertices.toArray();
        Arrays.sort(sortedVertices2);
        if (!Arrays.equals(sortedVertices1, sortedVertices2)) {
            return false;
        }

        Object[] sortedEdges1 = this.edges.toArray();
        Arrays.sort(sortedEdges1);
        Object[] sortedEdges2 = other.edges.toArray();
        Arrays.sort(sortedEdges2);
        return Arrays.equals(sortedEdges1, sortedEdges2);
    }

    @Override
    public String toString() {
        StringBuilder builder = new StringBuilder();
        for (Vertex<T> vertex : vertices) {
            builder.append(vertex.toString());
        }
        return builder.toString();
    }

    public static class Vertex<T extends Comparable<T>> implements Comparable<Vertex<T>> {

        private final T value;
        private int weight;
        private final List<Edge<T>> edges = new ArrayList<>();

        public Vertex(T value) {
            this(value, 0);
        }

        public Vertex(T value, int weight) {
            this.value = value;
            this.weight = weight;
        }

        /**
         * Deep copies the edges along with the value and weight.
         */
        public Vertex(Vertex<T> vertex) {
            this(vertex.getValue(), vertex.getWeight());
            this.edges.addAll(vertex.getEdges());
        }

        public T getValue() {
            return value;
        }

        public int getWeight() {
            return weight;
        }

        public void setWeight(int weight) {
            this.weight = weight;
        }

        public void addEdge(Edge<T> edge) {
            edges.add(edge);
        }

        public List<Edge<T>> getEdges() {
            return new ArrayList<>(edges);
        }

        public Edge<T> getEdge(Vertex<T> vertex) {
            for (Edge<T> edge : edges) {
                if (edge.getToVertex().equals(vertex)) {
                    return edge;
                }
            }
            return null;
        }

        public boolean pathTo(Vertex<T> vertex) {
            for (Edge<T> edge : edges) {
                if (edge.getToVertex().equals(vertex)) {
                    return true;
                }
            }
            return false;
        }

        @Override
        public int hashCode() {
            return Objects.hash(value, weight, edges);
        }

        @Override
        public boolean equals(Object obj) {
            if (this == obj) return true;
            if (obj == null || getClass() != obj.getClass()) return false;
            Vertex<?> other = (Vertex<?>) obj;
            if (this.weight != other.weight || this.edges.size() != other.edges.size() || !this.value.equals(other.value)) {
                return false;
            }
            Iterator<Edge<T>> iter1 = this.edges.iterator();
            Iterator<Edge<T>> iter2 = ((Vertex<T>) obj).edges.iterator();
            while (iter1.hasNext() && iter2.hasNext()) {
                if (iter1.next().getCost() != iter2.next().getCost()) {
                    return false;
                }
            }
            return true;
        }

        @Override
        public int compareTo(Vertex<T> other) {
            int valueComparison = this.value.compareTo(other.value);
            if (valueComparison != 0) return valueComparison;
            if (this.weight != other.weight) return Integer.compare(this.weight, other.weight);
            if (this.edges.size() != other.edges.size()) return Integer.compare(this.edges.size(), other.edges.size());
            Iterator<Edge<T>> iter1 = this.edges.iterator();
            Iterator<Edge<T>> iter2 = other.edges.iterator();
            while (iter1.hasNext() && iter2.hasNext()) {
                int costComparison = Integer.compare(iter1.next().getCost(), iter2.next().getCost());
                if (costComparison != 0) return costComparison;
            }
            return 0;
        }

        @Override
        public String toString() {
            StringBuilder builder = new StringBuilder();
            builder.append("Value=").append(value).append(" weight=").append(weight).append("\n");
            for (Edge<T> edge : edges) {
                builder.append("\t").append(edge.toString());
            }
            return builder.toString();
        }
    }

    public static class Edge<T extends Comparable<T>> implements Comparable<Edge<T>> {

        private final Vertex<T> from;
        private final Vertex<T> to;
        private int cost;

        public Edge(int cost, Vertex<T> from, Vertex<T> to) {
            if (from == null || to == null) {
                throw new NullPointerException("Both 'to' and 'from' vertices need to be non-NULL.");
            }
            this.cost = cost;
            this.from = from;
            this.to = to;
        }

        public Edge(Edge<T> edge) {
            this(edge.getCost(), edge.getFromVertex(), edge.getToVertex());
        }

        public int getCost() {
            return cost;
        }

        public void setCost(int cost) {
            this.cost = cost;
        }

        public Vertex<T> getFromVertex() {
            return from;
        }

        public Vertex<T> getToVertex() {
            return to;
        }

        @Override
        public int hashCode() {
            return Objects.hash(from, to, cost);
        }

        @Override
        public boolean equals(Object obj) {
            if (this == obj) return true;
            if (obj == null || getClass() != obj.getClass()) return false;
            Edge<?> other = (Edge<?>) obj;
            return this.cost == other.cost && this.from.equals(other.from) && this.to.equals(other.to);
        }

        @Override
        public int compareTo(Edge<T> other) {
            int costComparison = Integer.compare(this.cost, other.cost);
            if (costComparison != 0) return costComparison;
            int fromComparison = this.from.compareTo(other.from);
            if (fromComparison != 0) return fromComparison;
            return this.to.compareTo(other.to);
        }

        @Override
        public String toString() {
            return "[ " + from.value + "(" + from.weight + ") ] -> [ " + to.value + "(" + to.weight + ") ] = " + cost + "\n";
        }
    }

    public static class CostVertexPair<T extends Comparable<T>> implements Comparable<CostVertexPair<T>> {

        private int cost = Integer.MAX_VALUE;
        private final Vertex<T> vertex;

        public CostVertexPair(int cost, Vertex<T> vertex) {
            if (vertex == null) {
                throw new NullPointerException("vertex cannot be NULL.");
            }
            this.cost = cost;
            this.vertex = vertex;
        }

        public int getCost() {
            return cost;
        }

        public void setCost(int cost) {
            this.cost = cost;
        }

        public Vertex<T> getVertex() {
            return vertex;
        }

        @Override
        public int hashCode() {
            return Objects.hash(cost, vertex);
        }

        @Override
        public boolean equals(Object obj) {
            if (this == obj) return true;
            if (obj == null || getClass() != obj.getClass()) return false;
            CostVertexPair<?> other = (CostVertexPair<?>) obj;
            return this.cost == other.cost && this.vertex.equals(other.vertex);
        }

        @Override
        public int compareTo(CostVertexPair<T> other) {
            if (other == null) {
                throw new NullPointerException("CostVertexPair 'p' must be non-NULL.");
            }
            return Integer.compare(this.cost, other.cost);
        }

        @Override
        public String toString() {
            return vertex.getValue() + " (" + vertex.weight + ") cost=" + cost + "\n";
        }
    }

    public static class CostPathPair<T extends Comparable<T>> {

        private int cost;
        private final List<Edge<T>> path;

        public CostPathPair(int cost, List<Edge<T>> path) {
            if (path == null) {
                throw new NullPointerException("path cannot be NULL.");
            }
            this.cost = cost;
            this.path = path;
        }

        public int getCost() {
            return cost;
        }

        public void setCost(int cost) {
            this.cost = cost;
        }

        public List<Edge<T>> getPath() {
            return new ArrayList<>(path);
        }

        @Override
        public int hashCode() {
            return Objects.hash(cost, path);
        }

        @Override
        public boolean equals(Object obj) {
            if (this == obj) return true;
            if (obj == null || getClass() != obj.getClass()) return false;
            CostPathPair<?> other = (CostPathPair<?>) obj;
            if (this.cost != other.cost || this.path.size() != other.path.size()) return false;

            Iterator<Edge<T>> iter1 = this.path.iterator();
            Iterator<Edge<T>> iter2 = other.path.iterator();
            while (iter1.hasNext() && iter2.hasNext()) {
                if (!iter1.next().equals(iter2.next())) return false;
            }
            return true;
        }

        @Override
        public String toString() {
            StringBuilder builder = new StringBuilder();
            builder.append("Cost = ").append(cost).append("\n");
            for (Edge<T> edge : path) {
                builder.append("\t").append(edge);
            }
            return builder.toString();
        }
    }
}