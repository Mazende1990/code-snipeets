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

public class DfsSearchResultDeep extends SearchPhaseResult {

    private static final Term[] EMPTY_TERMS = new Term[0];
    private static final TermStatistics[] EMPTY_TERM_STATS = new TermStatistics[0];

    private Term[] terms;
    private TermStatistics[] termStatistics;
    private ObjectObjectHashMap<String, CollectionStatistics> fieldStatistics;
    private int maxDoc;

    public DfsSearchResult(StreamInput in) throws IOException {
        super(in);
        readFromStream(in);
    }

    public DfsSearchResult(ShardSearchContextId contextId, SearchShardTarget shardTarget, ShardSearchRequest shardSearchRequest) {
        setSearchShardTarget(shardTarget);
        this.contextId = contextId;
        setShardSearchRequest(shardSearchRequest);
        this.fieldStatistics = HppcMaps.newNoNullKeysMap();
    }

    public DfsSearchResult maxDoc(int maxDoc) {
        this.maxDoc = maxDoc;
        return this;
    }

    public int maxDoc() {
        return maxDoc;
    }

    public DfsSearchResult termsStatistics(Term[] terms, TermStatistics[] termStatistics) {
        this.terms = terms;
        this.termStatistics = termStatistics;
        return this;
    }

    public DfsSearchResult fieldStatistics(ObjectObjectHashMap<String, CollectionStatistics> fieldStatistics) {
        this.fieldStatistics = fieldStatistics;
        return this;
    }

    public Term[] terms() {
        return terms;
    }

    public TermStatistics[] termStatistics() {
        return termStatistics;
    }

    public ObjectObjectHashMap<String, CollectionStatistics> fieldStatistics() {
        return fieldStatistics;
    }

    @Override
    public void writeTo(StreamOutput out) throws IOException {
        contextId.writeTo(out);
        writeTerms(out);
        writeTermStats(out, termStatistics);
        writeFieldStats(out, fieldStatistics);
        out.writeVInt(maxDoc);
        writeShardSearchRequest(out);
    }

    private void readFromStream(StreamInput in) throws IOException {
        contextId = new ShardSearchContextId(in);
        terms = readTerms(in);
        termStatistics = readTermStats(in, terms);
        fieldStatistics = readFieldStats(in);
        maxDoc = in.readVInt();
        readShardSearchRequest(in);
    }

    private Term[] readTerms(StreamInput in) throws IOException {
        int termsSize = in.readVInt();
        if (termsSize == 0) {
            return EMPTY_TERMS;
        }
        Term[] terms = new Term[termsSize];
        for (int i = 0; i < termsSize; i++) {
            terms[i] = new Term(in.readString(), in.readBytesRef());
        }
        return terms;
    }

    private void writeTerms(StreamOutput out) throws IOException {
        out.writeVInt(terms.length);
        for (Term term : terms) {
            out.writeString(term.field());
            out.writeBytesRef(term.bytes());
        }
    }

    private void writeShardSearchRequest(StreamOutput out) throws IOException {
        if (out.getVersion().onOrAfter(Version.V_7_10_0)) {
            out.writeOptionalWriteable(getShardSearchRequest());
        }
    }

    private void readShardSearchRequest(StreamInput in) throws IOException {
        if (in.getVersion().onOrAfter(Version.V_7_10_0)) {
            setShardSearchRequest(in.readOptionalWriteable(ShardSearchRequest::new));
        }
    }

    public static void writeFieldStats(StreamOutput out, ObjectObjectHashMap<String, CollectionStatistics> fieldStatistics) throws IOException {
        out.writeVInt(fieldStatistics.size());
        for (ObjectObjectCursor<String, CollectionStatistics> cursor : fieldStatistics) {
            out.writeString(cursor.key);
            CollectionStatistics stats = cursor.value;
            out.writeVLong(stats.maxDoc());
            out.writeVLong(stats.docCount());
            out.writeVLong(stats.sumTotalTermFreq());
            out.writeVLong(stats.sumDocFreq());
        }
    }

    public static void writeTermStats(StreamOutput out, TermStatistics[] termStatistics) throws IOException {
        out.writeVInt(termStatistics.length);
        for (TermStatistics termStat : termStatistics) {
            writeSingleTermStats(out, termStat);
        }
    }

    public static void writeSingleTermStats(StreamOutput out, TermStatistics termStat) throws IOException {
        if (termStat != null) {
            out.writeVLong(termStat.docFreq());
            out.writeVLong(addOne(termStat.totalTermFreq()));
        } else {
            out.writeVLong(0);
            out.writeVLong(0);
        }
    }

    static ObjectObjectHashMap<String, CollectionStatistics> readFieldStats(StreamInput in) throws IOException {
        int numFieldStatistics = in.readVInt();
        ObjectObjectHashMap<String, CollectionStatistics> fieldStats = HppcMaps.newNoNullKeysMap(numFieldStatistics);
        for (int i = 0; i < numFieldStatistics; i++) {
            String field = in.readString();
            long maxDoc = in.readVLong();
            long docCount = in.readVLong();
            long sumTotalTermFreq = in.readVLong();
            long sumDocFreq = in.readVLong();
            fieldStats.put(field, new CollectionStatistics(field, maxDoc, docCount, sumTotalTermFreq, sumDocFreq));
        }
        return fieldStats;
    }

    static TermStatistics[] readTermStats(StreamInput in, Term[] terms) throws IOException {
        int termsStatsSize = in.readVInt();
        if (termsStatsSize == 0) {
            return EMPTY_TERM_STATS;
        }
        TermStatistics[] termStats = new TermStatistics[termsStatsSize];
        for (int i = 0; i < termsStatsSize; i++) {
            BytesRef term = terms[i].bytes();
            long docFreq = in.readVLong();
            long totalTermFreq = subOne(in.readVLong());
            if (docFreq > 0) {
                termStats[i] = new TermStatistics(term, docFreq, totalTermFreq);
            }
        }
        return termStats;
    }

    public static long addOne(long value) {
        assert value + 1 >= 0;
        return value + 1;
    }

    public static long subOne(long value) {
        assert value >= 0;
        return value - 1;
    }
}