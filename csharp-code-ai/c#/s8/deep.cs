using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures
{
    /// <summary>
    /// A collection of <see cref="DateTime"/> and <see cref="TValue"/> sorted by <see cref="DateTime"/>.
    /// </summary>
    /// <typeparam name="TValue">Value associated with a <see cref="DateTime"/>.</typeparam>
    public class Timeline<TValue> : ICollection<(DateTime Time, TValue Value)>, IEquatable<Timeline<TValue>>
    {
        private List<(DateTime Time, TValue Value)> _entries;

        #region Constructors

        public Timeline() => _entries = new List<(DateTime, TValue)>();

        public Timeline(DateTime time, TValue value) 
            : this() => Add(time, value);

        public Timeline(params TValue[] values)
            : this() => AddNow(values);

        public Timeline(params (DateTime, TValue)[] entries)
            : this() => Add(entries);

        #endregion

        #region Properties

        public int TimesCount => GetAllTimes().Length;
        public int ValuesCount => GetAllValues().Length;
        public int Count => _entries.Count;

        /// <summary>
        /// Gets or sets all values associated with the specified time.
        /// </summary>
        public TValue[] this[DateTime time]
        {
            get => GetValuesByTime(time);
            set
            {
                RemoveTime(time);
                Add(time, value);
            }
        }

        #endregion

        #region Public Methods

        #region Time-based Accessors

        public DateTime[] GetAllTimes() => _entries.Select(e => e.Time).Distinct().ToArray();
        
        public DateTime[] GetTimesByValue(TValue value) =>
            _entries.Where(e => e.Value.Equals(value))
                   .Select(e => e.Time)
                   .ToArray();

        public DateTime[] GetTimesBefore(DateTime time) => 
            GetAllTimes().Where(t => t < time).OrderBy(t => t).ToArray();

        public DateTime[] GetTimesAfter(DateTime time) => 
            GetAllTimes().Where(t => t > time).OrderBy(t => t).ToArray();

        #endregion

        #region Value-based Accessors

        public TValue[] GetAllValues() => _entries.Select(e => e.Value).ToArray();

        public TValue[] GetValuesByTime(DateTime time) =>
            _entries.Where(e => e.Time == time)
                   .Select(e => e.Value)
                   .ToArray();

        public Timeline<TValue> GetValuesBefore(DateTime time) =>
            new(_entries.Where(e => e.Time < time).ToArray());

        public Timeline<TValue> GetValuesAfter(DateTime time) =>
            new(_entries.Where(e => e.Time > time).ToArray());

        #endregion

        #region DateTime Component Filters

        public Timeline<TValue> GetValuesByMillisecond(int millisecond) =>
            FilterEntries(e => e.Time.Millisecond == millisecond);

        public Timeline<TValue> GetValuesBySecond(int second) =>
            FilterEntries(e => e.Time.Second == second);

        public Timeline<TValue> GetValuesByMinute(int minute) =>
            FilterEntries(e => e.Time.Minute == minute);

        public Timeline<TValue> GetValuesByHour(int hour) =>
            FilterEntries(e => e.Time.Hour == hour);

        public Timeline<TValue> GetValuesByDay(int day) =>
            FilterEntries(e => e.Time.Day == day);

        public Timeline<TValue> GetValuesByTimeOfDay(TimeSpan timeOfDay) =>
            FilterEntries(e => e.Time.TimeOfDay == timeOfDay);

        public Timeline<TValue> GetValuesByDayOfWeek(DayOfWeek dayOfWeek) =>
            FilterEntries(e => e.Time.DayOfWeek == dayOfWeek);

        public Timeline<TValue> GetValuesByDayOfYear(int dayOfYear) =>
            FilterEntries(e => e.Time.DayOfYear == dayOfYear);

        public Timeline<TValue> GetValuesByMonth(int month) =>
            FilterEntries(e => e.Time.Month == month);

        public Timeline<TValue> GetValuesByYear(int year) =>
            FilterEntries(e => e.Time.Year == year);

        #endregion

        #region Add Operations

        public void Add(DateTime time, TValue value)
        {
            _entries.Add((time, value));
            SortEntries();
        }

        public void Add(params (DateTime, TValue)[] entries)
        {
            _entries.AddRange(entries);
            SortEntries();
        }

        public void Add(Timeline<TValue> timeline) => Add(timeline.ToArray());

        public void AddNow(params TValue[] values)
        {
            var now = DateTime.Now;
            foreach (var value in values)
            {
                Add(now, value);
            }
        }

        #endregion

        #region Contains Operations

        public bool Contains(DateTime time, TValue value) => _entries.Contains((time, value));

        public bool Contains(params (DateTime, TValue)[] entries) => 
            entries.All(e => Contains(e.Item1, e.Item2));

        public bool Contains(Timeline<TValue> timeline) => Contains(timeline.ToArray());

        public bool ContainsTime(params DateTime[] times) => 
            times.All(t => GetAllTimes().Contains(t));

        public bool ContainsValue(params TValue[] values) => 
            values.All(v => GetAllValues().Contains(v));

        #endregion

        #region Remove Operations

        public bool Remove(DateTime time, TValue value) => _entries.Remove((time, value));

        public bool Remove(params (DateTime, TValue)[] entries) => 
            entries.Aggregate(false, (current, entry) => current | Remove(entry.Item1, entry.Item2));

        public bool Remove(Timeline<TValue> timeline) => Remove(timeline.ToArray());

        public bool RemoveTime(params DateTime[] times)
        {
            var hasTimes = times.All(t => GetAllTimes().Contains(t));
            if (hasTimes)
            {
                _entries = _entries.Where(e => !times.Contains(e.Time)).ToList();
            }
            return hasTimes;
        }

        public bool RemoveValue(params TValue[] values)
        {
            var hasValues = values.All(v => GetAllValues().Contains(v));
            if (hasValues)
            {
                _entries = _entries.Where(e => !values.Contains(e.Value)).ToList();
            }
            return hasValues;
        }

        #endregion

        #region Conversion Methods

        public (DateTime Time, TValue Value)[] ToArray() => _entries.ToArray();
        public IList<(DateTime Time, TValue Value)> ToList() => _entries.ToList();

        public IDictionary<DateTime, TValue> ToDictionary() => 
            _entries.ToDictionary(e => e.Time, e => e.Value);

        #endregion

        #endregion

        #region Interface Implementations

        public void Clear() => _entries.Clear();

        public void CopyTo((DateTime, TValue)[] array, int arrayIndex) => 
            _entries.CopyTo(array, arrayIndex);

        bool ICollection<(DateTime Time, TValue Value)>.IsReadOnly => false;

        void ICollection<(DateTime Time, TValue Value)>.Add((DateTime Time, TValue Value) item) => 
            Add(item.Time, item.Value);

        bool ICollection<(DateTime Time, TValue Value)>.Contains((DateTime Time, TValue Value) item) => 
            Contains(item.Time, item.Value);

        bool ICollection<(DateTime Time, TValue Value)>.Remove((DateTime Time, TValue Value) item) => 
            Remove(item.Time, item.Value);

        public IEnumerator<(DateTime Time, TValue Value)> GetEnumerator() => _entries.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region Equality Members

        public bool Equals(Timeline<TValue> other) => other is not null && this == other;

        public static bool operator ==(Timeline<TValue> left, Timeline<TValue> right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            
            var leftArray = left.ToArray();
            var rightArray = right.ToArray();
            
            return leftArray.Length == rightArray.Length && 
                   leftArray.SequenceEqual(rightArray);
        }

        public static bool operator !=(Timeline<TValue> left, Timeline<TValue> right) => !(left == right);

        public override bool Equals(object obj) => obj is Timeline<TValue> other && this == other;

        public override int GetHashCode() => _entries.GetHashCode();

        #endregion

        #region Private Helpers

        private void SortEntries() => 
            _entries = _entries.OrderBy(e => e.Time).ToList();

        private Timeline<TValue> FilterEntries(Func<(DateTime Time, TValue Value), bool> predicate) =>
            new(_entries.Where(predicate).ToArray());

        #endregion
    }
}