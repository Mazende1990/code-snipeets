/*
 * Copyright Elasticsearch B.V. and/or licensed to Elasticsearch B.V. under one
 * or more contributor license agreements. Licensed under the Elastic License
 * 2.0 and the Server Side Public License, v 1; you may not use this file except
 * in compliance with, at your election, the Elastic License 2.0 or the Server
 * Side Public License, v 1.
 */

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
 
 /**
  * Refactored version of {@code DfsSearchResult} preserving original functionality.
  */
 public class DfsSearchResult extends SearchPhaseResult {
 
     private static final Term[] EMPTY_TERMS = new Term[0];
     private static final TermStatistics[] EMPTY_TERM_STATS = new TermStatistics[0];
 
     private Term[] terms;
     private TermStatistics[] termStatistics;
     private ObjectObjectHashMap<String, CollectionStatistics> fieldStatistics = HppcMaps.newNoNullKeysMap();
     private int maxDoc;
 
     /**
      * Creates a {@code DfsSearchResult} by deserializing from a {@link StreamInput}.
      */
     public DfsSearchResult(StreamInput in) throws IOException {
         super(in);
         contextId = new ShardSearchContextId(in);
 
         int termsSize = in.readVInt();
         if (termsSize == 0) {
             terms = EMPTY_TERMS;
         } else {
             terms = new Term[termsSize];
             for (int i = 0; i < terms.length; i++) {
                 terms[i] = new Term(in.readString(), in.readBytesRef());
             }
         }
         termStatistics = readTermStats(in, terms);
 
         fieldStatistics = readFieldStats(in);
         maxDoc = in.readVInt();
 
         if (in.getVersion().onOrAfter(Version.V_7_10_0)) {
             setShardSearchRequest(in.readOptionalWriteable(ShardSearchRequest::new));
         }
     }
 
     /**
      * Creates a {@code DfsSearchResult} for the given context, shard target, and shard request.
      */
     public DfsSearchResult(ShardSearchContextId contextId,
                            SearchShardTarget shardTarget,
                            ShardSearchRequest shardSearchRequest) {
         setSearchShardTarget(shardTarget);
         this.contextId = contextId;
         setShardSearchRequest(shardSearchRequest);
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
         out.writeVInt(terms.length);
 
         for (Term term : terms) {
             out.writeString(term.field());
             out.writeBytesRef(term.bytes());
         }
         writeTermStats(out, termStatistics);
         writeFieldStats(out, fieldStatistics);
 
         out.writeVInt(maxDoc);
         if (out.getVersion().onOrAfter(Version.V_7_10_0)) {
             out.writeOptionalWriteable(getShardSearchRequest());
         }
     }
 
     public static void writeFieldStats(StreamOutput out,
                                        ObjectObjectHashMap<String, CollectionStatistics> fieldStatistics) throws IOException {
         out.writeVInt(fieldStatistics.size());
         for (ObjectObjectCursor<String, CollectionStatistics> c : fieldStatistics) {
             out.writeString(c.key);
             CollectionStatistics statistics = c.value;
             assert statistics.maxDoc() >= 0;
             out.writeVLong(statistics.maxDoc());
             out.writeVLong(statistics.docCount());
             out.writeVLong(statistics.sumTotalTermFreq());
             out.writeVLong(statistics.sumDocFreq());
         }
     }
 
     public static void writeTermStats(StreamOutput out, TermStatistics[] termStatistics) throws IOException {
         out.writeVInt(termStatistics.length);
         for (TermStatistics stats : termStatistics) {
             writeSingleTermStats(out, stats);
         }
     }
 
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
 
     static ObjectObjectHashMap<String, CollectionStatistics> readFieldStats(StreamInput in) throws IOException {
         int numFieldStatistics = in.readVInt();
         ObjectObjectHashMap<String, CollectionStatistics> statsMap =
                 HppcMaps.newNoNullKeysMap(numFieldStatistics);
         for (int i = 0; i < numFieldStatistics; i++) {
             String field = in.readString();
             long maxDoc = in.readVLong();
             long docCount = in.readVLong();
             long sumTotalTermFreq = in.readVLong();
             long sumDocFreq = in.readVLong();
             CollectionStatistics stats =
                     new CollectionStatistics(field, maxDoc, docCount, sumTotalTermFreq, sumDocFreq);
             statsMap.put(field, stats);
         }
         return statsMap;
     }
 
     static TermStatistics[] readTermStats(StreamInput in, Term[] terms) throws IOException {
         int termsStatsSize = in.readVInt();
         if (termsStatsSize == 0) {
             return EMPTY_TERM_STATS;
         }
 
         TermStatistics[] result = new TermStatistics[termsStatsSize];
         assert terms.length == termsStatsSize;
 
         for (int i = 0; i < result.length; i++) {
             BytesRef term = terms[i].bytes();
             long docFreq = in.readVLong();
             assert docFreq >= 0;
             long totalTermFreq = subOne(in.readVLong());
             if (docFreq != 0) {
                 result[i] = new TermStatistics(term, docFreq, totalTermFreq);
             }
         }
         return result;
     }
 
     /**
      * Since optional statistics are set to -1 by default in Lucene, we add one to ensure
      * that we do not store negative values.
      */
     public static long addOne(long value) {
         assert value + 1 >= 0;
         return value + 1;
     }
 
     /**
      * Subtract one from the statistic to restore its original value (since we added one
      * before serialization).
      */
     public static long subOne(long value) {
         assert value >= 0;
         return value - 1;
     }
 }
 