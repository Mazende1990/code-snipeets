package Java;

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

public class DfsSearchResult extends SearchPhaseResult {
    private static final Term[] EMPTY_TERMS = new Term[0];
    private static final TermStatistics[] EMPTY_TERM_STATS = new TermStatistics[0];
    
    private Term[] terms;
    private TermStatistics[] termStatistics;
    private ObjectObjectHashMap<String, CollectionStatistics> fieldStatistics = HppcMaps.newNoNullKeysMap();
    private int maxDoc;

    /**
     * Constructor to deserialize from input stream.
     */
    public DfsSearchResult(StreamInput in) throws IOException {
        super(in);
        contextId = new ShardSearchContextId(in);
        terms = readTerms(in);
        termStatistics = readTermStats(in, terms);
        fieldStatistics = readFieldStats(in);
        maxDoc = in.readVInt();

        if (in.getVersion().onOrAfter(Version.V_7_10_0)) {
            setShardSearchRequest(in.readOptionalWriteable(ShardSearchRequest::new));
        }
    }

    /**
     * Constructor for creating a new DFS search result.
     */
    public DfsSearchResult(ShardSearchContextId contextId, SearchShardTarget shardTarget, ShardSearchRequest shardSearchRequest) {
        this.setSearchShardTarget(shardTarget);
        this.contextId = contextId;
        setShardSearchRequest(shardSearchRequest);
    }

    public DfsSearchResult setMaxDoc(int maxDoc) {
        this.maxDoc = maxDoc;
        return this;
    }

    public int getMaxDoc() {
        return maxDoc;
    }

    public DfsSearchResult setTermsStatistics(Term[] terms, TermStatistics[] termStatistics) {
        this.terms = terms;
        this.termStatistics = termStatistics;
        return this;
    }

    public DfsSearchResult setFieldStatistics(ObjectObjectHashMap<String, CollectionStatistics> fieldStatistics) {
        this.fieldStatistics = fieldStatistics;
        return this;
    }

    public Term[] getTerms() {
        return terms;
    }

    public TermStatistics[] getTermStatistics() {
        return termStatistics;
    }

    public ObjectObjectHashMap<String, CollectionStatistics> getFieldStatistics() {
        return fieldStatistics;
    }

    @Override
    public void writeTo(StreamOutput out) throws IOException {
        contextId.writeTo(out);
        writeTerms(out, terms);
        writeTermStats(out, termStatistics);
        writeFieldStats(out, fieldStatistics);
        out.writeVInt(maxDoc);
        
        if (out.getVersion().onOrAfter(Version.V_7_10_0)) {
            out.writeOptionalWriteable(getShardSearchRequest());
        }
    }

    /**
     * Reads terms from input stream.
     */
    private static Term[] readTerms(StreamInput in) throws IOException {
        int termsSize = in.readVInt();
        if (termsSize == 0) {
            return EMPTY_TERMS;
        }
        Term[] terms = new Term[termsSize];
        for (int i = 0; i < terms.length; i++) {
            terms[i] = new Term(in.readString(), in.readBytesRef());
        }
        return terms;
    }

    /**
     * Reads field statistics from input stream.
     */
    private static ObjectObjectHashMap<String, CollectionStatistics> readFieldStats(StreamInput in) throws IOException {
        int numFieldStats = in.readVInt();
        ObjectObjectHashMap<String, CollectionStatistics> fieldStats = HppcMaps.newNoNullKeysMap(numFieldStats);
        for (int i = 0; i < numFieldStats; i++) {
            String field = in.readString();
            CollectionStatistics stats = new CollectionStatistics(field, in.readVLong(), in.readVLong(), in.readVLong(), in.readVLong());
            fieldStats.put(field, stats);
        }
        return fieldStats;
    }

    /**
     * Reads term statistics from input stream.
     */
    private static TermStatistics[] readTermStats(StreamInput in, Term[] terms) throws IOException {
        int termsStatsSize = in.readVInt();
        if (termsStatsSize == 0) {
            return EMPTY_TERM_STATS;
        }
        TermStatistics[] termStatistics = new TermStatistics[termsStatsSize];
        for (int i = 0; i < termStatistics.length; i++) {
            long docFreq = in.readVLong();
            long totalTermFreq = decrement(in.readVLong());
            if (docFreq > 0) {
                termStatistics[i] = new TermStatistics(terms[i].bytes(), docFreq, totalTermFreq);
            }
        }
        return termStatistics;
    }

    /**
     * Writes field statistics to output stream.
     */
    public static void writeFieldStats(StreamOutput out, ObjectObjectHashMap<String, CollectionStatistics> fieldStats) throws IOException {
        out.writeVInt(fieldStats.size());
        for (ObjectObjectCursor<String, CollectionStatistics> entry : fieldStats) {
            out.writeString(entry.key);
            CollectionStatistics stats = entry.value;
            out.writeVLong(stats.maxDoc());
            out.writeVLong(stats.docCount());
            out.writeVLong(stats.sumTotalTermFreq());
            out.writeVLong(stats.sumDocFreq());
        }
    }

    /**
     * Utility method to increment a value safely.
     */
    public static long increment(long value) {
        return value + 1;
    }

    /**
     * Utility method to decrement a value safely.
     */
    public static long decrement(long value) {
        return value - 1;
    }
}
