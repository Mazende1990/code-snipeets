package java.s4;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collection;
import java.util.Iterator;
import java.util.List;

/**
 * GraphCo. Could be directed or undirected depending on the TYPE enum. A GraphCo is
 * an abstract representation of a set of objects where some pairs of the
 * objects are connected by links.
 * <p>
 * @see <a href="https://en.wikipedia.org/wiki/GraphCo_(mathematics)">GraphCo (Wikipedia)</a>
 * <br>
 * @author Justin Wetherell <phishman3579@gmail.com>
 */
@SuppressWarnings("unchecked")
public class GraphCo<T extends Comparable<T>> {

    private List<Vertex<T>> allVertices = new ArrayList<>();
    private List<Edge<T>> allEdges = new ArrayList<>();
    private TYPE type = TYPE.UNDIRECTED;

    public enum TYPE {
        DIRECTED, UNDIRECTED
    }

    public GraphCo() { }

    public GraphCo(TYPE type) {
        this.type = type;
    }

    /** Deep copies **/
    public GraphCo(GraphCo<T> g) {
        this.type = g.getType();
        for (Vertex<T> v : g.getVertices()) {
            this.allVertices.add(new Vertex<>(v));
        }
        for (Vertex<T> v : this.getVertices()) {
            for (Edge<T> e : v.getEdges()) {
                this.allEdges.add(e);
            }
        }
    }

    /**
     * Creates a GraphCo from the vertices and edges. This defaults to an undirected GraphCo
     * 
     * NOTE: Duplicate vertices and edges ARE allowed.
     * NOTE: Copies the vertex and edge objects but does NOT store the Collection parameters itself.
     * 
     * @param vertices Collection of vertices
     * @param edges Collection of edges
     */
    public GraphCo(Collection<Vertex<T>> vertices, Collection<Edge<T>> edges) {
        this(TYPE.UNDIRECTED, vertices, edges);
    }

    /**
     * Creates a GraphCo from the vertices and edges.
     * 
     * NOTE: Duplicate vertices and edges ARE allowed.
     * NOTE: Copies the vertex and edge objects but does NOT store the Collection parameters itself.
     * 
     * @param vertices Collection of vertices
     * @param edges Collection of edges
     */
    public GraphCo(TYPE type, Collection<Vertex<T>> vertices, Collection<Edge<T>> edges) {
        this(type);
        this.allVertices.addAll(vertices);
        this.allEdges.addAll(edges);
        for (Edge<T> e : edges) {
            final Vertex<T> from = e.from;
            final Vertex<T> to = e.to;
            if (!this.allVertices.contains(from) || !this.allVertices.contains(to)) continue;
            from.addEdge(e);
            if (this.type == TYPE.UNDIRECTED) {
                Edge<T> reciprocal = new Edge<>(e.cost, to, from);
                to.addEdge(reciprocal);
                this.allEdges.add(reciprocal);
            }
        }
    }

    public TYPE getType() {
        return type;
    }

    public List<Vertex<T>> getVertices() {
        return allVertices;
    }

    public List<Edge<T>> getEdges() {
        return allEdges;
    }

    @Override
    public int hashCode() {
        int code = this.type.hashCode() + this.allVertices.size() + this.allEdges.size();
        for (Vertex<T> v : allVertices) {
            code *= v.hashCode();
        }
        for (Edge<T> e : allEdges) {
            code *= e.hashCode();
        }
        return 31 * code;
    }

    @Override
    public boolean equals(Object g1) {
        if (!(g1 instanceof GraphCo)) return false;
        final GraphCo<T> g = (GraphCo<T>) g1;
        if (this.type != g.type) return false;
        if (this.allVertices.size() != g.allVertices.size()) return false;
        if (this.allEdges.size() != g.allEdges.size()) return false;

        final Object[] ov1 = this.allVertices.toArray();
        Arrays.sort(ov1);
        final Object[] ov2 = g.allVertices.toArray();
        Arrays.sort(ov2);
        for (int i = 0; i < ov1.length; i++) {
            if (!ov1[i].equals(ov2[i])) return false;
        }

        final Object[] oe1 = this.allEdges.toArray();
        Arrays.sort(oe1);
        final Object[] oe2 = g.allEdges.toArray();
        Arrays.sort(oe2);
        for (int i = 0; i < oe1.length; i++) {
            if (!oe1[i].equals(oe2[i])) return false;
        }

        return true;
    }

    @Override
    public String toString() {
        final StringBuilder builder = new StringBuilder();
        for (Vertex<T> v : allVertices) {
            builder.append(v.toString());
        }
        return builder.toString();
    }

    public static class Vertex<T extends Comparable<T>> implements Comparable<Vertex<T>> {

        private T value;
        private int weight;
        private List<Edge<T>> edges = new ArrayList<>();

        public Vertex(T value) {
            this.value = value;
        }

        public Vertex(T value, int weight) {
            this(value);
            this.weight = weight;
        }

        /** Deep copies the edges along with the value and weight **/
        public Vertex(Vertex<T> vertex) {
            this(vertex.value, vertex.weight);
            this.edges.addAll(vertex.edges);
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

        public void addEdge(Edge<T> e) {
            edges.add(e);
        }

        public List<Edge<T>> getEdges() {
            return edges;
        }

        public Edge<T> getEdge(Vertex<T> v) {
            for (Edge<T> e : edges) {
                if (e.to.equals(v)) return e;
            }
            return null;
        }

        public boolean pathTo(Vertex<T> v) {
            for (Edge<T> e : edges) {
                if (e.to.equals(v)) return true;
            }
            return false;
        }

        @Override
        public int hashCode() {
            final int code = this.value.hashCode() + this.weight + this.edges.size();
            return 31 * code;
        }

        @Override
        public boolean equals(Object v1) {
            if (!(v1 instanceof Vertex)) return false;
            final Vertex<T> v = (Vertex<T>) v1;
            if (this.weight != v.weight) return false;
            if (this.edges.size() != v.edges.size()) return false;
            if (!this.value.equals(v.value)) return false;

            final Iterator<Edge<T>> iter1 = this.edges.iterator();
            final Iterator<Edge<T>> iter2 = v.edges.iterator();
            while (iter1.hasNext() && iter2.hasNext()) {
                if (iter1.next().cost != iter2.next().cost) return false;
            }
            return true;
        }

        @Override
        public int compareTo(Vertex<T> v) {
            int valueComp = this.value.compareTo(v.value);
            if (valueComp != 0) return valueComp;
            if (this.weight != v.weight) return Integer.compare(this.weight, v.weight);
            if (this.edges.size() != v.edges.size()) return Integer.compare(this.edges.size(), v.edges.size());

            final Iterator<Edge<T>> iter1 = this.edges.iterator();
            final Iterator<Edge<T>> iter2 = v.edges.iterator();
            while (iter1.hasNext() && iter2.hasNext()) {
                int costComp = Integer.compare(iter1.next().cost, iter2.next().cost);
                if (costComp != 0) return costComp;
            }
            return 0;
        }

        @Override
        public String toString() {
            final StringBuilder builder = new StringBuilder();
            builder.append("Value=").append(value).append(" weight=").append(weight).append("\n");
            for (Edge<T> e : edges) {
                builder.append("\t").append(e.toString());
            }
            return builder.toString();
        }
    }

    public static class Edge<T extends Comparable<T>> implements Comparable<Edge<T>> {

        private Vertex<T> from;
        private Vertex<T> to;
        private int cost;

        public Edge(int cost, Vertex<T> from, Vertex<T> to) {
            if (from == null || to == null) {
                throw new NullPointerException("Both 'to' and 'from' vertices need to be non-NULL.");
            }
            this.cost = cost;
            this.from = from;
            this.to = to;
        }

        public Edge(Edge<T> e) {
            this(e.cost, e.from, e.to);
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
            final int cost = (this.cost * (this.getFromVertex().hashCode() * this.getToVertex().hashCode())); 
            return 31 * cost;
        }

        @Override
        public boolean equals(Object e1) {
            if (!(e1 instanceof Edge)) return false;
            final Edge<T> e = (Edge<T>) e1;
            if (this.cost != e.cost) return false;
            if (!this.from.equals(e.from)) return false;
            if (!this.to.equals(e.to)) return false;
            return true;
        }

        @Override
        public int compareTo(Edge<T> e) {
            if (this.cost != e.cost) return Integer.compare(this.cost, e.cost);
            int fromComp = this.from.compareTo(e.from);
            if (fromComp != 0) return fromComp;
            return this.to.compareTo(e.to);
        }

        @Override
        public String toString() {
            return String.format("[ %s(%d) ] -> [ %s(%d) ] = %d\n", from.value, from.weight, to.value, to.weight, cost);
        }
    }

    public static class CostVertexPair<T extends Comparable<T>> implements Comparable<CostVertexPair<T>> {

        private int cost = Integer.MAX_VALUE;
        private Vertex<T> vertex;

        public CostVertexPair(int cost, Vertex<T> vertex) {
            if (vertex == null) throw new NullPointerException("vertex cannot be NULL.");
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
            return 31 * (this.cost * ((this.vertex != null) ? this.vertex.hashCode() : 1));
        }

        @Override
        public boolean equals(Object e1) {
            if (!(e1 instanceof CostVertexPair)) return false;
            final CostVertexPair<?> pair = (CostVertexPair<?>) e1;
            if (this.cost != pair.cost) return false;
            return this.vertex.equals(pair.vertex);
        }

        @Override
        public int compareTo(CostVertexPair<T> p) {
            if (p == null) throw new NullPointerException("CostVertexPair 'p' must be non-NULL.");
            return Integer.compare(this.cost, p.cost);
        }

        @Override
        public String toString() {
            return String.format("%s (%d) cost=%d\n", vertex.getValue(), vertex.weight, cost);
        }
    }

    public static class CostPathPair<T extends Comparable<T>> {

        private int cost;
        private List<Edge<T>> path;

        public CostPathPair(int cost, List<Edge<T>> path) {
            if (path == null) throw new NullPointerException("path cannot be NULL.");
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
            return path;
        }

        @Override
        public int hashCode() {
            int hash = this.cost;
            for (Edge<T> e : path) {
                hash *= e.cost;
            }
            return 31 * hash;
        }

        @Override
        public boolean equals(Object obj) {
            if (!(obj instanceof CostPathPair)) return false;
            final CostPathPair<?> pair = (CostPathPair<?>) obj;
            if (this.cost != pair.cost) return false;

            final Iterator<?> iter1 = this.getPath().iterator();
            final Iterator<?> iter2 = pair.getPath().iterator();
            while (iter1.hasNext() && iter2.hasNext()) {
                if (!iter1.next().equals(iter2.next())) return false;
            }
            return true;
        }

        @Override
        public String toString() {
            final StringBuilder builder = new StringBuilder();
            builder.append("Cost = ").append(cost).append("\n");
            for (Edge<T> e : path) {
                builder.append("\t").append(e);
            }
            return builder.toString();
        }
    }
}
