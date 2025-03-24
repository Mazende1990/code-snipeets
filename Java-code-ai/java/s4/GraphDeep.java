package java.s4;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collection;
import java.util.Iterator;
import java.util.List;

/**
 * GraphDeep. Could be directed or undirected depending on the TYPE enum. A GraphDeep is
 * an abstract representation of a set of objects where some pairs of the
 * objects are connected by links.
 * <p>
 * @see <a href="https://en.wikipedia.org/wiki/GraphDeep_(mathematics)">GraphDeep (Wikipedia)</a>
 * <br>
 * @author Justin Wetherell <phishman3579@gmail.com>
 */
@SuppressWarnings("unchecked")
public class GraphDeep<T extends Comparable<T>> {

    private List<Vertex<T>> vertices = new ArrayList<>();
    private List<Edge<T>> edges = new ArrayList<>();

    public enum Type {
        DIRECTED, UNDIRECTED
    }

    private Type GraphDeepType = Type.UNDIRECTED;

    public GraphDeep() { }

    public GraphDeep(Type GraphDeepType) {
        this.GraphDeepType = GraphDeepType;
    }

    public GraphDeep(GraphDeep<T> GraphDeep) {
        this.GraphDeepType = GraphDeep.getType();

        // Copy the vertices which also copies the edges
        for (Vertex<T> vertex : GraphDeep.getVertices()) {
            this.vertices.add(new Vertex<>(vertex));
        }

        for (Vertex<T> vertex : this.getVertices()) {
            for (Edge<T> edge : vertex.getEdges()) {
                this.edges.add(edge);
            }
        }
    }

    public GraphDeep(Collection<Vertex<T>> vertices, Collection<Edge<T>> edges) {
        this(Type.UNDIRECTED, vertices, edges);
    }

    public GraphDeep(Type GraphDeepType, Collection<Vertex<T>> vertices, Collection<Edge<T>> edges) {
        this(GraphDeepType);

        this.vertices.addAll(vertices);
        this.edges.addAll(edges);

        for (Edge<T> edge : edges) {
            Vertex<T> from = edge.getFromVertex();
            Vertex<T> to = edge.getToVertex();

            if (!this.vertices.contains(from) || !this.vertices.contains(to)) {
                continue;
            }

            from.addEdge(edge);
            if (this.GraphDeepType == Type.UNDIRECTED) {
                Edge<T> reciprocal = new Edge<>(edge.getCost(), to, from);
                to.addEdge(reciprocal);
                this.edges.add(reciprocal);
            }
        }
    }

    public Type getType() {
        return GraphDeepType;
    }

    public List<Vertex<T>> getVertices() {
        return vertices;
    }

    public List<Edge<T>> getEdges() {
        return edges;
    }

    @Override
    public int hashCode() {
        int code = GraphDeepType.hashCode() + vertices.size() + edges.size();
        for (Vertex<T> vertex : vertices) {
            code *= vertex.hashCode();
        }
        for (Edge<T> edge : edges) {
            code *= edge.hashCode();
        }
        return 31 * code;
    }

    @Override
    public boolean equals(Object obj) {
        if (!(obj instanceof GraphDeep)) {
            return false;
        }

        GraphDeep<T> GraphDeep = (GraphDeep<T>) obj;

        if (this.GraphDeepType != GraphDeep.GraphDeepType) {
            return false;
        }

        if (this.vertices.size() != GraphDeep.vertices.size()) {
            return false;
        }

        if (this.edges.size() != GraphDeep.edges.size()) {
            return false;
        }

        Object[] thisVerticesArray = this.vertices.toArray();
        Arrays.sort(thisVerticesArray);
        Object[] otherVerticesArray = GraphDeep.vertices.toArray();
        Arrays.sort(otherVerticesArray);

        for (int i = 0; i < thisVerticesArray.length; i++) {
            Vertex<T> thisVertex = (Vertex<T>) thisVerticesArray[i];
            Vertex<T> otherVertex = (Vertex<T>) otherVerticesArray[i];
            if (!thisVertex.equals(otherVertex)) {
                return false;
            }
        }

        Object[] thisEdgesArray = this.edges.toArray();
        Arrays.sort(thisEdgesArray);
        Object[] otherEdgesArray = GraphDeep.edges.toArray();
        Arrays.sort(otherEdgesArray);

        for (int i = 0; i < thisEdgesArray.length; i++) {
            Edge<T> thisEdge = (Edge<T>) thisEdgesArray[i];
            Edge<T> otherEdge = (Edge<T>) otherEdgesArray[i];
            if (!thisEdge.equals(otherEdge)) {
                return false;
            }
        }

        return true;
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

        public void addEdge(Edge<T> edge) {
            edges.add(edge);
        }

        public List<Edge<T>> getEdges() {
            return edges;
        }

        public Edge<T> getEdge(Vertex<T> vertex) {
            for (Edge<T> edge : edges) {
                if (edge.getToVertex().equals(vertex)) {
                    return edge;
                }
            }
            return null;
        }

        public boolean hasPathTo(Vertex<T> vertex) {
            for (Edge<T> edge : edges) {
                if (edge.getToVertex().equals(vertex)) {
                    return true;
                }
            }
            return false;
        }

        @Override
        public int hashCode() {
            int code = value.hashCode() + weight + edges.size();
            return 31 * code;
        }

        @Override
        public boolean equals(Object obj) {
            if (!(obj instanceof Vertex)) {
                return false;
            }

            Vertex<T> vertex = (Vertex<T>) obj;

            if (this.weight != vertex.weight) {
                return false;
            }

            if (this.edges.size() != vertex.edges.size()) {
                return false;
            }

            if (!this.value.equals(vertex.value)) {
                return false;
            }

            Iterator<Edge<T>> thisEdgesIterator = this.edges.iterator();
            Iterator<Edge<T>> otherEdgesIterator = vertex.edges.iterator();
            while (thisEdgesIterator.hasNext() && otherEdgesIterator.hasNext()) {
                Edge<T> thisEdge = thisEdgesIterator.next();
                Edge<T> otherEdge = otherEdgesIterator.next();
                if (thisEdge.getCost() != otherEdge.getCost()) {
                    return false;
                }
            }

            return true;
        }

        @Override
        public int compareTo(Vertex<T> vertex) {
            int valueComparison = this.value.compareTo(vertex.value);
            if (valueComparison != 0) {
                return valueComparison;
            }

            if (this.weight < vertex.weight) {
                return -1;
            }
            if (this.weight > vertex.weight) {
                return 1;
            }

            if (this.edges.size() < vertex.edges.size()) {
                return -1;
            }
            if (this.edges.size() > vertex.edges.size()) {
                return 1;
            }

            Iterator<Edge<T>> thisEdgesIterator = this.edges.iterator();
            Iterator<Edge<T>> otherEdgesIterator = vertex.edges.iterator();
            while (thisEdgesIterator.hasNext() && otherEdgesIterator.hasNext()) {
                Edge<T> thisEdge = thisEdgesIterator.next();
                Edge<T> otherEdge = otherEdgesIterator.next();
                if (thisEdge.getCost() < otherEdge.getCost()) {
                    return -1;
                }
                if (thisEdge.getCost() > otherEdge.getCost()) {
                    return 1;
                }
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

        private Vertex<T> from;
        private Vertex<T> to;
        private int cost;

        public Edge(int cost, Vertex<T> from, Vertex<T> to) {
            if (from == null || to == null) {
                throw new NullPointerException("Both 'from' and 'to' vertices must be non-null.");
            }

            this.cost = cost;
            this.from = from;
            this.to = to;
        }

        public Edge(Edge<T> edge) {
            this(edge.cost, edge.from, edge.to);
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
            int code = cost * (from.hashCode() * to.hashCode());
            return 31 * code;
        }

        @Override
        public boolean equals(Object obj) {
            if (!(obj instanceof Edge)) {
                return false;
            }

            Edge<T> edge = (Edge<T>) obj;

            if (this.cost != edge.cost) {
                return false;
            }

            if (!this.from.equals(edge.from)) {
                return false;
            }

            if (!this.to.equals(edge.to)) {
                return false;
            }

            return true;
        }

        @Override
        public int compareTo(Edge<T> edge) {
            if (this.cost < edge.cost) {
                return -1;
            }
            if (this.cost > edge.cost) {
                return 1;
            }

            int fromComparison = this.from.compareTo(edge.from);
            if (fromComparison != 0) {
                return fromComparison;
            }

            return this.to.compareTo(edge.to);
        }

        @Override
        public String toString() {
            return String.format("[ %s(%d) ] -> [ %s(%d) ] = %d\n", 
                                from.getValue(), from.getWeight(), 
                                to.getValue(), to.getWeight(), 
                                cost);
        }
    }

    public static class CostVertexPair<T extends Comparable<T>> implements Comparable<CostVertexPair<T>> {

        private int cost;
        private Vertex<T> vertex;

        public CostVertexPair(int cost, Vertex<T> vertex) {
            if (vertex == null) {
                throw new NullPointerException("Vertex cannot be null.");
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
            return 31 * (cost * ((vertex != null) ? vertex.hashCode() : 1));
        }

        @Override
        public boolean equals(Object obj) {
            if (!(obj instanceof CostVertexPair)) {
                return false;
            }

            CostVertexPair<?> pair = (CostVertexPair<?>) obj;
            if (this.cost != pair.cost) {
                return false;
            }

            return this.vertex.equals(pair.vertex);
        }

        @Override
        public int compareTo(CostVertexPair<T> pair) {
            if (pair == null) {
                throw new NullPointerException("CostVertexPair must be non-null.");
            }

            if (this.cost < pair.cost) {
                return -1;
            }
            if (this.cost > pair.cost) {
                return 1;
            }
            return 0;
        }

        @Override
        public String toString() {
            return String.format("%s (%d) cost=%d\n", vertex.getValue(), vertex.getWeight(), cost);
        }
    }

    public static class CostPathPair<T extends Comparable<T>> {

        private int cost;
        private List<Edge<T>> path;

        public CostPathPair(int cost, List<Edge<T>> path) {
            if (path == null) {
                throw new NullPointerException("Path cannot be null.");
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
            return path;
        }

        @Override
        public int hashCode() {
            int hash = cost;
            for (Edge<T> edge : path) {
                hash *= edge.getCost();
            }
            return 31 * hash;
        }

        @Override
        public boolean equals(Object obj) {
            if (!(obj instanceof CostPathPair)) {
                return false;
            }

            CostPathPair<?> pair = (CostPathPair<?>) obj;
            if (this.cost != pair.cost) {
                return false;
            }

            Iterator<?> thisPathIterator = this.path.iterator();
            Iterator<?> otherPathIterator = pair.path.iterator();
            while (thisPathIterator.hasNext() && otherPathIterator.hasNext()) {
                Edge<T> thisEdge = (Edge<T>) thisPathIterator.next();
                Edge<T> otherEdge = (Edge<T>) otherPathIterator.next();
                if (!thisEdge.equals(otherEdge)) {
                    return false;
                }
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