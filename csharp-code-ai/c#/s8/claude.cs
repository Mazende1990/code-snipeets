using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures
{
    /// <summary>
    /// A collection of DateTime and TValue pairs, sorted chronologically.
    /// </summary>
    /// <typeparam name="TValue">The type of value associated with each DateTime.</typeparam>
    public class Timeline<TValue> : 
        ICollection<(DateTime Time, TValue Value)>, 
        IEquatable<Timeline<TValue>>
    {
        private List<(DateTime Time, TValue Value)> _entries;

        // Constructors
        public Timeline() => _entries = new List<(DateTime, TValue)>();

        public Timeline(DateTime time, TValue value) : this() 
        {
            Add(time, value);
        }
 
        public Timeline(params TValue[] values) : this()
        {
            AddNow(values);
        }

        public Timeline(params (DateTime, TValue)[] entries)
        {
            _entries = entries
                .OrderBy(pair => pair.Item1)
                .ToList();
        }

        // Core Properties and Collection Interface
        public int Count => _entries.Count;
        public bool IsReadOnly => false;

        // Indexer
        public TValue[] this[DateTime time]
        {
            get => GetValuesByTime(time);
            set
            {
                _entries.RemoveAll(entry => entry.Time == time);
                foreach (var val in value)
                {
                    Add(time, val);
                }
            }
        }

        // Basic Collection Methods
        public void Clear() => _entries.Clear();

        public void CopyTo((DateTime, TValue)[] array, int arrayIndex) =>
            _entries.CopyTo(array, arrayIndex);

        // Enumeration Support
        public IEnumerator<(DateTime Time, TValue Value)> GetEnumerator() => 
            _entries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // Adding Entries
        public void Add(DateTime time, TValue value)
        {
            _entries.Add((time, value));
            _entries = _entries.OrderBy(pair => pair.Time).ToList();
        }

        public void Add(params (DateTime, TValue)[] entries)
        {
            _entries.AddRange(entries);
            _entries = _entries.OrderBy(pair => pair.Time).ToList();
        }

        public void Add(Timeline<TValue> timeline) => 
            Add(timeline.ToArray());

        public void AddNow(params TValue[] values)
        {
            var now = DateTime.Now;
            foreach (var value in values)
            {
                Add(now, value);
            }
        }

        // Extraction Methods
        public DateTime[] GetAllTimes() => 
            _entries.Select(e => e.Time).Distinct().ToArray();

        public TValue[] GetAllValues() => 
            _entries.Select(e => e.Value).ToArray();

        public TValue[] GetValuesByTime(DateTime time) => 
            _entries.Where(e => e.Time == time)
                    .Select(e => e.Value)
                    .ToArray();

        // Time-based Filtering Methods
        public Timeline<TValue> GetValuesBefore(DateTime time) => 
            new Timeline<TValue>(_entries.Where(e => e.Time < time).ToArray());

        public Timeline<TValue> GetValuesAfter(DateTime time) => 
            new Timeline<TValue>(_entries.Where(e => e.Time > time).ToArray());

        // Temporal Filtering Methods
        public Timeline<TValue> GetValuesByMillisecond(int millisecond) => 
            new Timeline<TValue>(_entries.Where(e => e.Time.Millisecond == millisecond).ToArray());

        public Timeline<TValue> GetValuesBySecond(int second) => 
            new Timeline<TValue>(_entries.Where(e => e.Time.Second == second).ToArray());

        // Other methods follow similar pattern...
        // (GetValuesByMinute, GetValuesByHour, etc.)

        // Containment Checks
        public bool Contains(DateTime time, TValue value) => 
            _entries.Contains((time, value));

        public bool Contains(params (DateTime, TValue)[] entries) => 
            entries.All(entry => Contains(entry.Item1, entry.Item2));

        public bool Contains(Timeline<TValue> timeline) => 
            Contains(timeline.ToArray());

        // Removal Methods
        public bool Remove(DateTime time, TValue value) => 
            _entries.Remove((time, value));

        public bool Remove(params (DateTime, TValue)[] entries) => 
            entries.Aggregate(false, (result, entry) => result | Remove(entry.Item1, entry.Item2));

        public bool Remove(Timeline<TValue> timeline) => 
            Remove(timeline.ToArray());

        // Conversion Methods
        public (DateTime Time, TValue Value)[] ToArray() => 
            _entries.ToArray();

        public IList<(DateTime Time, TValue Value)> ToList() => 
            _entries;

        public IDictionary<DateTime, TValue> ToDictionary() => 
            _entries.ToDictionary(e => e.Time, e => e.Value);

        // Equality and Comparison
        public bool Equals(Timeline<TValue>? other) => 
            other is not null && this == other;

        public override bool Equals(object? obj) => 
            obj is Timeline<TValue> timeline && this == timeline;

        public override int GetHashCode() => 
            _entries.GetHashCode();

        public static bool operator ==(Timeline<TValue> left, Timeline<TValue> right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;

            return left._entries.SequenceEqual(right._entries);
        }

        public static bool operator !=(Timeline<TValue> left, Timeline<TValue> right) => 
            !(left == right);

        // Interface Methods
        void ICollection<(DateTime, TValue)>.Add((DateTime, TValue) item) => 
            Add(item.Time, item.Value);

        bool ICollection<(DateTime, TValue)>.Contains((DateTime, TValue) item) => 
            Contains(item.Time, item.Value);

        bool ICollection<(DateTime, TValue)>.Remove((DateTime, TValue) item) => 
            Remove(item.Time, item.Value);
    }
}