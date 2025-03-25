using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures
{
    /// <summary>
    /// A collection of <see cref="DateTime" /> and <see cref="TValue" /> sorted by <see cref="DateTime" />.
    /// </summary>
    /// <typeparam name="TValue">The value associated with a <see cref="DateTime" />.</typeparam>
    public class Timeline<TValue> : ICollection<(DateTime Time, TValue Value)>, IEquatable<Timeline<TValue>>
    {
        private List<(DateTime Time, TValue Value)> timeline = new();

        public Timeline() { }

        public Timeline(DateTime time, TValue value)
        {
            timeline.Add((time, value));
        }

        public Timeline(params TValue[] values)
        {
            var now = DateTime.Now;
            timeline.AddRange(values.Select(v => (now, v)));
        }

        public Timeline(params (DateTime, TValue)[] entries)
        {
            timeline = entries.OrderBy(e => e.Item1).ToList();
        }

        public int Count => timeline.Count;
        public int TimesCount => GetAllTimes().Length;
        public int ValuesCount => GetAllValues().Length;
        public bool IsReadOnly => false;

        public TValue[] this[DateTime time]
        {
            get => GetValuesByTime(time);
            set
            {
                timeline.RemoveAll(pair => pair.Time == time);
                foreach (var v in value)
                {
                    Add(time, v);
                }
            }
        }

        public void Add(DateTime time, TValue value)
        {
            timeline.Add((time, value));
            timeline.Sort((a, b) => a.Time.CompareTo(b.Time));
        }

        public void Add(params (DateTime, TValue)[] entries)
        {
            timeline.AddRange(entries);
            timeline.Sort((a, b) => a.Time.CompareTo(b.Time));
        }

        public void Add(Timeline<TValue> other)
        {
            Add(other.ToArray());
        }

        public void AddNow(params TValue[] values)
        {
            var now = DateTime.Now;
            foreach (var value in values)
                Add(now, value);
        }

        public void Clear() => timeline.Clear();

        public bool Contains(DateTime time, TValue value) => timeline.Contains((time, value));

        public bool Contains(params (DateTime, TValue)[] entries) =>
            entries.All(e => Contains(e.Item1, e.Item2));

        public bool Contains(Timeline<TValue> other) => Contains(other.ToArray());

        public bool ContainsTime(params DateTime[] times) =>
            times.All(t => timeline.Any(e => e.Time == t));

        public bool ContainsValue(params TValue[] values) =>
            values.All(v => timeline.Any(e => e.Value!.Equals(v)));

        public bool Remove(DateTime time, TValue value) => timeline.Remove((time, value));

        public bool Remove(params (DateTime, TValue)[] entries)
        {
            bool result = false;
            foreach (var (time, value) in entries)
                result |= timeline.Remove((time, value));
            return result;
        }

        public bool Remove(Timeline<TValue> other) => Remove(other.ToArray());

        public bool RemoveTime(params DateTime[] times)
        {
            var removedAny = timeline.RemoveAll(pair => times.Contains(pair.Time)) > 0;
            return removedAny;
        }

        public bool RemoveValue(params TValue[] values)
        {
            var removedAny = timeline.RemoveAll(pair => values.Contains(pair.Value)) > 0;
            return removedAny;
        }

        public DateTime[] GetAllTimes() => timeline.Select(t => t.Time).Distinct().ToArray();

        public TValue[] GetAllValues() => timeline.Select(t => t.Value).ToArray();

        public TValue[] GetValuesByTime(DateTime time) =>
            timeline.Where(pair => pair.Time == time).Select(pair => pair.Value).ToArray();

        public DateTime[] GetTimesByValue(TValue value) =>
            timeline.Where(pair => pair.Value!.Equals(value)).Select(pair => pair.Time).ToArray();

        public DateTime[] GetTimesBefore(DateTime time) =>
            timeline.Where(p => p.Time < time).Select(p => p.Time).Distinct().OrderBy(t => t).ToArray();

        public DateTime[] GetTimesAfter(DateTime time) =>
            timeline.Where(p => p.Time > time).Select(p => p.Time).Distinct().OrderBy(t => t).ToArray();

        public Timeline<TValue> GetValuesBefore(DateTime time) =>
            new Timeline<TValue>(timeline.Where(pair => pair.Time < time).ToArray());

        public Timeline<TValue> GetValuesAfter(DateTime time) =>
            new Timeline<TValue>(timeline.Where(pair => pair.Time > time).ToArray());

        public Timeline<TValue> GetValuesByMillisecond(int ms) =>
            new Timeline<TValue>(timeline.Where(pair => pair.Time.Millisecond == ms).ToArray());

        public Timeline<TValue> GetValuesBySecond(int sec) =>
            new Timeline<TValue>(timeline.Where(pair => pair.Time.Second == sec).ToArray());

        public Timeline<TValue> GetValuesByMinute(int min) =>
            new Timeline<TValue>(timeline.Where(pair => pair.Time.Minute == min).ToArray());

        public Timeline<TValue> GetValuesByHour(int hour) =>
            new Timeline<TValue>(timeline.Where(pair => pair.Time.Hour == hour).ToArray());

        public Timeline<TValue> GetValuesByDay(int day) =>
            new Timeline<TValue>(timeline.Where(pair => pair.Time.Day == day).ToArray());

        public Timeline<TValue> GetValuesByTimeOfDay(TimeSpan timeOfDay) =>
            new Timeline<TValue>(timeline.Where(pair => pair.Time.TimeOfDay == timeOfDay).ToArray());

        public Timeline<TValue> GetValuesByDayOfWeek(DayOfWeek dayOfWeek) =>
            new Timeline<TValue>(timeline.Where(pair => pair.Time.DayOfWeek == dayOfWeek).ToArray());

        public Timeline<TValue> GetValuesByDayOfYear(int dayOfYear) =>
            new Timeline<TValue>(timeline.Where(pair => pair.Time.DayOfYear == dayOfYear).ToArray());

        public Timeline<TValue> GetValuesByMonth(int month) =>
            new Timeline<TValue>(timeline.Where(pair => pair.Time.Month == month).ToArray());

        public Timeline<TValue> GetValuesByYear(int year) =>
            new Timeline<TValue>(timeline.Where(pair => pair.Time.Year == year).ToArray());

        public (DateTime Time, TValue Value)[] ToArray() => timeline.ToArray();

        public IList<(DateTime Time, TValue Value)> ToList() => timeline;

        public IDictionary<DateTime, TValue> ToDictionary()
        {
            var dict = new Dictionary<DateTime, TValue>();
            foreach (var (date, value) in timeline)
            {
                dict[date] = value;
            }
            return dict;
        }

        public bool Equals(Timeline<TValue>? other)
        {
            if (other is null) return false;
            return this == other;
        }

        public static bool operator ==(Timeline<TValue> left, Timeline<TValue> right)
        {
            var leftArray = left.ToArray();
            var rightArray = right.ToArray();
            if (leftArray.Length != rightArray.Length) return false;

            for (int i = 0; i < leftArray.Length; i++)
            {
                if (leftArray[i].Time != rightArray[i].Time ||
                    !EqualityComparer<TValue>.Default.Equals(leftArray[i].Value, rightArray[i].Value))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator !=(Timeline<TValue> left, Timeline<TValue> right) => !(left == right);

        public override bool Equals(object? obj) => obj is Timeline<TValue> timeline && this == timeline;

        public override int GetHashCode() => timeline.GetHashCode();

        public void CopyTo((DateTime, TValue)[] array, int arrayIndex) =>
            timeline.CopyTo(array, arrayIndex);

        public IEnumerator<(DateTime Time, TValue Value)> GetEnumerator() => timeline.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void ICollection<(DateTime Time, TValue Value)>.Add((DateTime Time, TValue Value) item) =>
            Add(item.Time, item.Value);

        bool ICollection<(DateTime Time, TValue Value)>.Contains((DateTime Time, TValue Value) item) =>
            Contains(item.Time, item.Value);

        bool ICollection<(DateTime Time, TValue Value)>.Remove((DateTime Time, TValue Value) item) =>
            Remove(item.Time, item.Value);
    }
}
