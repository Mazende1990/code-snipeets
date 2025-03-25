using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures
{
    /// <summary>
    /// A collection of <see cref="DateTime" /> and <see cref="TValue" /> sorted by <see cref="DateTime" /> field.
    /// </summary>
    /// <typeparam name="TValue">Value associated with a <see cref="DateTime" />.</typeparam>
    public class Timeline<TValue> : ICollection<(DateTime Time, TValue Value)>, IEquatable<Timeline<TValue>>
    {
        private List<(DateTime Time, TValue Value)> _timeline = new();

        public Timeline() => _timeline = new List<(DateTime, TValue)>();

        public Timeline(DateTime time, TValue value) => _timeline = new List<(DateTime, TValue)> { (time, value) };

        public Timeline(params TValue[] values)
        {
            var now = DateTime.Now;
            _timeline.AddRange(values.Select(value => (now, value)));
        }

        public Timeline(params (DateTime Time, TValue Value)[] timelineItems)
        {
            _timeline = timelineItems.OrderBy(item => item.Time).ToList();
        }

        public int TimesCount => GetAllTimes().Length;
        public int ValuesCount => GetAllValues().Length;

        /// <summary>
        /// Get all values associated with <paramref name="time" />.
        /// </summary>
        /// <param name="time">Time to get values for.</param>
        /// <returns>Values associated with <paramref name="time" />.</returns>
        public TValue[] this[DateTime time]
        {
            get => GetValuesByTime(time);
            set
            {
                _timeline.RemoveAll(item => item.Time == time);
                _timeline.AddRange(value.Select(v => (time, v)));
                _timeline = _timeline.OrderBy(item => item.Time).ToList();
            }
        }

        bool ICollection<(DateTime Time, TValue Value)>.IsReadOnly => false;

        /// <summary>
        /// Gets the count of pairs.
        /// </summary>
        public int Count => _timeline.Count;

        public void Clear() => _timeline.Clear();

        /// <summary>
        /// Copy a value to an array.
        /// </summary>
        /// <param name="array">Destination array.</param>
        /// <param name="arrayIndex">The start index.</param>
        public void CopyTo((DateTime Time, TValue Value)[] array, int arrayIndex) => _timeline.CopyTo(array, arrayIndex);

        void ICollection<(DateTime Time, TValue Value)>.Add((DateTime Time, TValue Value) item) => Add(item.Time, item.Value);

        bool ICollection<(DateTime Time, TValue Value)>.Contains((DateTime Time, TValue Value) item) => Contains(item.Time, item.Value);

        bool ICollection<(DateTime Time, TValue Value)>.Remove((DateTime Time, TValue Value) item) => Remove(item.Time, item.Value);

        IEnumerator IEnumerable.GetEnumerator() => _timeline.GetEnumerator();

        IEnumerator<(DateTime Time, TValue Value)> IEnumerable<(DateTime Time, TValue Value)>.GetEnumerator() => _timeline.GetEnumerator();

        public bool Equals(Timeline<TValue>? other) => other is not null && this == other;

        public static bool operator ==(Timeline<TValue> left, Timeline<TValue> right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;

            var leftArray = left.ToArray();
            var rightArray = right.ToArray();

            if (leftArray.Length != rightArray.Length) return false;

            return leftArray.SequenceEqual(rightArray);
        }

        public static bool operator !=(Timeline<TValue> left, Timeline<TValue> right) => !(left == right);

        /// <summary>
        /// Get all <see cref="DateTime" /> of the timeline.
        /// </summary>
        public DateTime[] GetAllTimes() => _timeline.Select(item => item.Time).Distinct().ToArray();

        /// <summary>
        /// Get <see cref="DateTime" /> values of the timeline that have this <paramref name="value" />.
        /// </summary>
        public DateTime[] GetTimesByValue(TValue value) =>
            _timeline.Where(item => Equals(item.Value, value)).Select(item => item.Time).ToArray();

        /// <summary>
        /// Get all <see cref="DateTime" /> before <paramref name="time" />.
        /// </summary>
        public DateTime[] GetTimesBefore(DateTime time) => GetAllTimes().Where(t => t < time).OrderBy(t => t).ToArray();

        /// <summary>
        /// Get all <see cref="DateTime" /> after <paramref name="time" />.
        /// </summary>
        public DateTime[] GetTimesAfter(DateTime time) => GetAllTimes().Where(t => t > time).OrderBy(t => t).ToArray();

        /// <summary>
        /// Get all <see cref="TValue" /> of the timeline.
        /// </summary>
        public TValue[] GetAllValues() => _timeline.Select(item => item.Value).ToArray();

        /// <summary>
        /// Get all <see cref="TValue" /> associated with <paramref name="time" />.
        /// </summary>
        public TValue[] GetValuesByTime(DateTime time) => _timeline.Where(item => item.Time == time).Select(item => item.Value).ToArray();

        /// <summary>
        /// Get all <see cref="TValue" /> before <paramref name="time" />.
        /// </summary>
        public Timeline<TValue> GetValuesBefore(DateTime time) => new(_timeline.Where(item => item.Time < time).ToArray());

        /// <summary>
        /// Get all <see cref="TValue" /> after <paramref name="time" />.
        /// </summary>
        public Timeline<TValue> GetValuesAfter(DateTime time) => new(_timeline.Where(item => item.Time > time).ToArray());

        /// <summary>
        /// Gets all values that happened at specified millisecond.
        /// </summary>
        public Timeline<TValue> GetValuesByMillisecond(int millisecond) => new(_timeline.Where(item => item.Time.Millisecond == millisecond).ToArray());

        /// <summary>
        /// Gets all values that happened at specified second.
        /// </summary>
        public Timeline<TValue> GetValuesBySecond(int second) => new(_timeline.Where(item => item.Time.Second == second).ToArray());

        /// <summary>
        /// Gets all values that happened at specified minute.
        /// </summary>
        public Timeline<TValue> GetValuesByMinute(int minute) => new(_timeline.Where(item => item.Time.Minute == minute).ToArray());

        /// <summary>
        /// Gets all values that happened at specified hour.
        /// </summary>
        public Timeline<TValue> GetValuesByHour(int hour) => new(_timeline.Where(item => item.Time.Hour == hour).ToArray());

        /// <summary>
        /// Gets all values that happened at specified day.
        /// </summary>
        public Timeline<TValue> GetValuesByDay(int day) => new(_timeline.Where(item => item.Time.Day == day).ToArray());

        /// <summary>
        /// Gets all values that happened at specified time of the day.
        /// </summary>
        public Timeline<TValue> GetValuesByTimeOfDay(TimeSpan timeOfDay) => new(_timeline.Where(item => item.Time.TimeOfDay == timeOfDay).ToArray());

        /// <summary>
        /// Gets all values that happened at specified day of the week.
        /// </summary>
        public Timeline<TValue> GetValuesByDayOfWeek(DayOfWeek dayOfWeek) => new(_timeline.Where(item => item.Time.DayOfWeek == dayOfWeek).ToArray());

        /// <summary>
        /// Gets all values that happened at specified day of the year.
        /// </summary>
        public Timeline<TValue> GetValuesByDayOfYear(int dayOfYear) => new(_timeline.Where(item => item.Time.DayOfYear == dayOfYear).ToArray());

        /// <summary>
        /// Gets all values that happened at specified month.
        /// </summary>
        public Timeline<TValue> GetValuesByMonth(int month) => new(_timeline.Where(item => item.Time.Month == month).ToArray());

        /// <summary>
        /// Gets all values that happened at specified year.
        /// </summary>
                public Timeline<TValue> GetValuesByYear(int year) => new(_timeline.Where(item => item.Time.Year == year).ToArray());

        /// <summary>
        /// Add a <see cref="DateTime" /> and a <see cref="TValue" /> to the timeline.
        /// </summary>
        public void Add(DateTime time, TValue value)
        {
            _timeline.Add((time, value));
            _timeline = _timeline.OrderBy(item => item.Time).ToList();
        }

        /// <summary>
        /// Add a set of <see cref="DateTime" /> and <see cref="TValue" /> to the timeline.
        /// </summary>
        public void Add(params (DateTime Time, TValue Value)[] timelineItems)
        {
            _timeline.AddRange(timelineItems);
            _timeline = _timeline.OrderBy(item => item.Time).ToList();
        }

        /// <summary>
        /// Add an existing timeline to this timeline.
        /// </summary>
        public void Add(Timeline<TValue> timeline) => Add(timeline.ToArray());

        /// <summary>
        /// Add a <paramref name="value" /> associated with <see cref="DateTime.Now" /> to the timeline.
        /// </summary>
        public void AddNow(params TValue[] values)
        {
            var now = DateTime.Now;
            foreach (var value in values)
            {
                Add(now, value);
            }
        }

        /// <summary>
        /// Returns true if the timeline contains this value pair.
        /// </summary>
        public bool Contains(DateTime time, TValue value) => _timeline.Contains((time, value));

        /// <summary>
        /// Returns true if the timeline contains this set of value pairs.
        /// </summary>
        public bool Contains(params (DateTime Time, TValue Value)[] timelineItems) => timelineItems.All(item => Contains(item.Time, item.Value));

        /// <summary>
        /// Returns true if this timeline contains an existing timeline.
        /// </summary>
        public bool Contains(Timeline<TValue> timeline) => Contains(timeline.ToArray());

        /// <summary>
        /// Returns true if the timeline contains <paramref name="time" />.
        /// </summary>
        public bool ContainsTime(params DateTime[] times) => times.All(time => GetAllTimes().Contains(time));

        /// <summary>
        /// Returns true if the timeline contains <paramref name="value" />.
        /// </summary>
        public bool ContainsValue(params TValue[] values) => values.All(value => GetAllValues().Contains(value));

        /// <summary>
        /// Remove a value pair from the timeline.
        /// </summary>
        /// <returns>Returns true if the operation completed successfully.</returns>
        public bool Remove(DateTime time, TValue value) => _timeline.Remove((time, value));

        /// <summary>
        /// Remove a set of value pairs from the timeline.
        /// </summary>
        /// <returns>Returns true if the operation completed successfully.</returns>
        public bool Remove(params (DateTime Time, TValue Value)[] timelineItems) => timelineItems.Aggregate(false, (result, item) => result | _timeline.Remove(item));

        /// <summary>
        /// Remove an existing timeline from this timeline.
        /// </summary>
        /// <returns>Returns true if the operation completed successfully.</returns>
        public bool Remove(Timeline<TValue> timeline) => Remove(timeline.ToArray());

        /// <summary>
        /// Remove a value pair from the timeline if the time is equal to <paramref name="time" />.
        /// </summary>
        /// <returns>Returns true if the operation completed successfully.</returns>
        public bool RemoveTime(params DateTime[] times)
        {
            var removed = false;
            if (times.Any(time => GetAllTimes().Contains(time)))
            {
                _timeline.RemoveAll(item => times.Contains(item.Time));
                removed = true;
            }
            return removed;
        }

        /// <summary>
        /// Remove a value pair from the timeline if the value is equal to <paramref name="value" />.
        /// </summary>
        /// <returns>Returns true if the operation completed successfully.</returns>
        public bool RemoveValue(params TValue[] values)
        {
            var removed = false;
            if (values.Any(value => GetAllValues().Contains(value)))
            {
                _timeline.RemoveAll(item => values.Contains(item.Value));
                removed = true;
            }
            return removed;
        }

        /// <summary>
        /// Convert the timeline to an array.
        /// </summary>
        public (DateTime Time, TValue Value)[] ToArray() => _timeline.ToArray();

        /// <summary>
        /// Convert the timeline to a list.
        /// </summary>
        public IList<(DateTime Time, TValue Value)> ToList() => _timeline;

        /// <summary>
        /// Convert the timeline to a dictionary.
        /// </summary>
        public IDictionary<DateTime, TValue> ToDictionary() => _timeline.ToDictionary(item => item.Time, item => item.Value);

        public override bool Equals(object? obj) => obj is Timeline<TValue> timeline && this == timeline;

        public override int GetHashCode() => _timeline.GetHashCode();
    }
}