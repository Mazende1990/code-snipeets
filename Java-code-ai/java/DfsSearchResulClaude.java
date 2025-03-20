package javacodehuman;

import com.carrotsearch.hppc.ObjectObjectHashMap;
import com.carrotsearch.hppc.cursors.ObjectObjectCursor;
import org.apache.lucene.index.Term;
import org.apache.lucene.search.CollectionStatistics;
import org.apache.lucene.search.TermStatistics;
import org.apache.lucene.util.BytesRef;
import org.elasticsearch.Version;
import org.elasticsearch.common.collect.HppcMaps;
import org.elasticsearch.common.io.stream.StreamInput;
import org.elasticsearch.common.io.stream.StreamOutput;
import org.elasticsearch.search.SearchPhaseResult;
import org.elasticsearch.search.SearchShardTarget;
import org.elasticsearch.search.internal.ShardSearchContextId;
import org.elasticsearch.search.internal.ShardSearchRequest;

import java.io.IOException;

/**
 * Represents the results of a distributed frequency search (DFS) phase.
 * This class stores term statistics, field statistics, and document counts
 * needed for score normalization across shards.
 */
public class DfsSearchResulClaude extends SearchPhaseResult {

    // Constants
    private static final Term[] EMPTY_TERMS = new Term[0];
    private static final TermStatistics[] EMPTY_TERM_STATS = new TermStatistics[0];
    
    // Fields
    private Term[] terms;
    private TermStatistics[] termStatistics;
    private ObjectObjectHashMap<String, CollectionStatistics> fieldStatistics;
    private int maxDoc;

    /**
     * Creates a new DfsSearchResult from a stream input.
     *
     * @param in The stream input to read from
     * @throws IOException If an I/O error occurs
     */
    public DfsSearchResult(StreamInput in) throws IOException {
        super(in);
        contextId = new ShardSearchContextId(in);
        
        // Read terms
        terms = readTerms(in);
        
        // Read term statistics
        termStatistics = readTermStats(in, terms);
        
        // Read field statistics
        fieldStatistics = readFieldStats(in);
        
        // Read max doc count
        maxDoc = in.readVInt();
        
        // Read shard search request (for versions >= 7.10.0)
        if (in.getVersion().onOrAfter(Version.V_7_10_0)) {
            setShardSearchRequest(in.readOptionalWriteable(ShardSearchRequest::new));
        }
    }

    /**
     * Creates a new DfsSearchResult with the given context ID and shard target.
     *
     * @param contextId The search context ID
     * @param shardTarget The search shard target
     * @param shardSearchRequest The shard search request
     */
    public DfsSearchResult(ShardSearchContextId contextId, SearchShardTarget shardTarget, ShardSearchRequest shardSearchRequest) {
        this.setSearchShardTarget(shardTarget);
        this.contextId = contextId;
        setShardSearchRequest(shardSearchRequest);
        this.fieldStatistics = HppcMaps.newNoNullKeysMap();
    }

    // Getter and setter methods with fluent API

    /**
     * Sets the maximum document count.
     *
     * @param maxDoc The maximum document count
     * @return This DfsSearchResult instance for method chaining
     */
    public DfsSearchResult maxDoc(int maxDoc) {
        this.maxDoc = maxDoc;
        return this;
    }

    /**
     * Gets the maximum document count.
     *
     * @return The maximum document count
     */
    public int maxDoc() {
        return maxDoc;
    }

    /**
     * Sets the terms and term statistics.
     *
     * @param terms The terms
     * @param termStatistics The term statistics
     * @return This DfsSearchResult instance for method chaining
     */
    public DfsSearchResult termsStatistics(Term[] terms, TermStatistics[] termStatistics) {
        this.terms = terms;
        this.termStatistics = termStatistics;
        return this;
    }

    /**
     * Sets the field statistics.
     *
     * @param fieldStatistics The field statistics
     * @return This DfsSearchResult instance for method chaining
     */
    public DfsSearchResult fieldStatistics(ObjectObjectHashMap<String, CollectionStatistics> fieldStatistics) {
        this.fieldStatistics = fieldStatistics;
        return this;
    }

    /**
     * Gets the terms.
     *
     * @return The terms
     */
    public Term[] terms() {
        return terms;
    }

    /**
     * Gets the term statistics.
     *
     * @return The term statistics
     */
    public TermStatistics[] termStatistics() {
        return termStatistics;
    }

    /**
     * Gets the field statistics.
     *
     * @return The field statistics
     */
    public ObjectObjectHashMap<String, CollectionStatistics> fieldStatistics() {
        return fieldStatistics;
    }

    /**
     * Writes this DfsSearchResult to a stream output.
     *
     * @param out The stream output to write to
     * @throws IOException If an I/O error occurs
     */
    @Override
    public void writeTo(StreamOutput out) throws IOException {
        contextId.writeTo(out);
        
        // Write terms
        writeTerms(out);
        
        // Write term statistics
        writeTermStats(out, termStatistics);
        
        // Write field statistics
        writeFieldStats(out, fieldStatistics);
        
        // Write max doc count
        out.writeVInt(maxDoc);
        
        // Write shard search request (for versions >= 7.10.0)
        if (out.getVersion().onOrAfter(Version.V_7_10_0)) {
            out.writeOptionalWriteable(getShardSearchRequest());
        }
    }

    // Private helper methods for reading/writing

    private Term[] readTerms(StreamInput in) throws IOException {
        int termsSize = in.readVInt();
        if (termsSize == 0) {
            return EMPTY_TERMS;
        }
        
        Term[] result = new Term[termsSize];
        for (int i = 0; i < result.length; i++) {
            result[i] = new Term(in.readString(), in.readBytesRef());
        }
        return result;
    }

    private void writeTerms(StreamOutput out) throws IOException {
        out.writeVInt(terms.length);
        for (Term term : terms) {
            out.writeString(term.field());
            out.writeBytesRef(term.bytes());
        }
    }

    /**
     * Writes field statistics to a stream output.
     *
     * @param out The stream output to write to
     * @param fieldStatistics The field statistics to write
     * @throws IOException If an I/O error occurs
     */
    public static void writeFieldStats(StreamOutput out, ObjectObjectHashMap<String, CollectionStatistics> fieldStatistics) 
            throws IOException {
        out.writeVInt(fieldStatistics.size());

        for (ObjectObjectCursor<String, CollectionStatistics> c : fieldStatistics) {
            out.writeString(c.key);
            CollectionStatistics statistics = c.value;
            assert statistics.maxDoc() >= 0;
            out.writeVLong(statistics.maxDoc());
            // stats are always positive numbers
            out.writeVLong(statistics.docCount());
            out.writeVLong(statistics.sumTotalTermFreq());
            out.writeVLong(statistics.sumDocFreq());
        }
    }

    /**
     * Writes term statistics to a stream output.
     *
     * @param out The stream output to write to
     * @param termStatistics The term statistics to write
     * @throws IOException If an I/O error occurs
     */
    public static void writeTermStats(StreamOutput out, TermStatistics[] termStatistics) throws IOException {
        out.writeVInt(termStatistics.length);
        for (TermStatistics termStatistic : termStatistics) {
            writeSingleTermStats(out, termStatistic);
        }
    }

    /**
     * Writes a single term statistic to a stream output.
     *
     * @param out The stream output to write to
     * @param termStatistic The term statistic to write
     * @throws IOException If an I/O error occurs
     */
    public static void writeSingleTermStats(StreamOutput out, TermStatistics termStatistic) throws IOException {
        if (termStatistic != null) {
            assert termStatistic.docFreq() > 0;
            out.writeVLong(termStatistic.docFreq());
            out.writeVLong(addOne(termStatistic.totalTermFreq()));
        } else {
            out.writeVLong(0);
            out.writeVLong(0);
        }
    }

    /**
     * Reads field statistics from a stream input.
     *
     * @param in The stream input to read from
     * @return The field statistics
     * @throws IOException If an I/O error occurs
     */
    static ObjectObjectHashMap<String, CollectionStatistics> readFieldStats(StreamInput in) throws IOException {
        final int numFieldStatistics = in.readVInt();
        ObjectObjectHashMap<String, CollectionStatistics> fieldStatistics = HppcMaps.newNoNullKeysMap(numFieldStatistics);
        
        for (int i = 0; i < numFieldStatistics; i++) {
            final String field = in.readString();
            assert field != null;
            final long maxDoc = in.readVLong();
            // stats are always positive numbers
            final long docCount = in.readVLong();
            final long sumTotalTermFreq = in.readVLong();
            final long sumDocFreq = in.readVLong();
            
            CollectionStatistics stats = new CollectionStatistics(field, maxDoc, docCount, sumTotalTermFreq, sumDocFreq);
            fieldStatistics.put(field, stats);
        }
        
        return fieldStatistics;
    }

    /**
     * Reads term statistics from a stream input.
     *
     * @param in The stream input to read from
     * @param terms The terms
     * @return The term statistics
     * @throws IOException If an I/O error occurs
     */
    static TermStatistics[] readTermStats(StreamInput in, Term[] terms) throws IOException {
        int termsStatsSize = in.readVInt();
        
        if (termsStatsSize == 0) {
            return EMPTY_TERM_STATS;
        }
        
        TermStatistics[] termStatistics = new TermStatistics[termsStatsSize];
        assert terms.length == termsStatsSize;
        
        for (int i = 0; i < termStatistics.length; i++) {
            BytesRef term = terms[i].bytes();
            final long docFreq = in.readVLong();
            assert docFreq >= 0;
            final long totalTermFreq = subOne(in.readVLong());
            
            if (docFreq != 0) {
                termStatistics[i] = new TermStatistics(term, docFreq, totalTermFreq);
            }
        }
        
        return termStatistics;
    }

    /**
     * Adds one to a value for encoding purposes.
     * Optional statistics are set to -1 in Lucene by default.
     * Since we are using var longs to encode values, we add one to each value
     * to ensure we don't waste space and don't add negative values.
     *
     * @param value The value to add one to
     * @return The value plus one
     */
    public static long addOne(long value) {
        assert value + 1 >= 0;
        return value + 1;
    }

    /**
     * Subtracts one from a value for decoding purposes.
     * See {@link #addOne(long)} for more information.
     *
     * @param value The value to subtract one from
     * @return The value minus one
     */
    public static long subOne(long value) {
        assert value >= 0;
        return value - 1;
    }
}