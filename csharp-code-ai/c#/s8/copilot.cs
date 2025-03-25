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
        private List<(DateTime Time, TValue Value)> timeline = new();

        // Constructor overloads
        public Timeline() => timeline = new List<(DateTime, TValue)>();

        public Timeline(DateTime time, TValue value) => timeline = new List<(DateTime, TValue)> { (time, value) };

        public Timeline(params TValue[] values)
        {
            var now = DateTime.Now;
            foreach (var value in values)
            {
                timeline.Add((now, value));
            }
        }

        public Timeline(params (DateTime, TValue)[] timeline)
        {
            this.timeline = timeline.OrderBy(pair => pair.Time).ToList();
        }

        // Properties
        public int TimesCount => GetAllTimes().Length;

        public int ValuesCount => GetAllValues().Length;

        public TValue[] this[DateTime time]
        {
            get => GetValuesByTime(time);
            set
            {
                timeline.RemoveAll(pair => pair.Time == time);
                foreach (var value in value)
                {
                    Add(time, value);
                }
            }
        }

        bool ICollection<(DateTime Time, TValue Value)>.IsReadOnly => false;

        public int Count => timeline.Count;

        // Methods
        public void Clear() => timeline.Clear();

        public void CopyTo((DateTime, TValue)[] array, int arrayIndex) => timeline.CopyTo(array, arrayIndex);

        void ICollection<(DateTime Time, TValue Value)>.Add((DateTime Time, TValue Value) item) => Add(item.Time, item.Value);

        bool ICollection<(DateTime Time, TValue Value)>.Contains((DateTime Time, TValue Value) item) => Contains(item.Time, item.Value);

        bool ICollection<(DateTime Time, TValue Value)>.Remove((DateTime Time, TValue Value) item) => Remove(item.Time, item.Value);

        IEnumerator IEnumerable.GetEnumerator() => timeline.GetEnumerator();

        IEnumerator<(DateTime Time, TValue Value)> IEnumerable<(DateTime Time, TValue Value)>.GetEnumerator() => timeline.GetEnumerator();

        public bool Equals(Timeline<TValue>? other) => other is not null && this == other;

        public static bool operator ==(Timeline<TValue> left, Timeline<TValue> right)
        {
            var leftArray = left.ToArray();
            var rightArray = right.ToArray();
            if (leftArray.Length == rightArray.Length)
            {
                for (var i = 0; i < leftArray.Length; i++)
                {
                    if (leftArray[i].Time != rightArray[i].Time || !leftArray[i].Value!.Equals(rightArray[i].Value))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public static bool operator !=(Timeline<TValue> left, Timeline<TValue> right) => !(left == right);

        // Helper methods
        public DateTime[] GetAllTimes() => timeline.Select(t => t.Time).Distinct().ToArray();

        public DateTime[] GetTimesByValue(TValue value) => timeline.Where(pair => pair.Value!.Equals(value)).Select(pair => pair.Time).ToArray();

        public DateTime[] GetTimesBefore(DateTime time) => GetAllTimes().Where(t => t < time).OrderBy(t => t).ToArray();

        public DateTime[] GetTimesAfter(DateTime time) => GetAllTimes().Where(t => t > time).OrderBy(t => t).ToArray();

        GetAllValues() => timeline.Select(pair => pair.Value).ToArray();

        public TValue[] GetValuesByTime(DateTime time) => timeline.Where(pair => pair.Time == time).Select(pair => pair.Value).ToArray();

        public Timeline<TValue> GetValuesBefore(DateTime time) => new Timeline<TValue>(timeline.Where(pair => pair.Time < time).ToArray());

        public Timeline<TValue> GetValuesAfter(DateTime time) => new Timeline<TValue>(timeline.Where(pair => pair.Time > time).ToArray());

        public Timeline<TValue> GetValuesByMillisecond(int millisecond) => new Timeline<TValue>(timeline.Where(pair => pair.Time.Millisecond == millisecond).ToArray());

        public Timeline<TValue> GetValuesBySecond(int second) => new Timeline<TValue>(timeline.Where(pair => pair.Time.Second == second).ToArray());

        public Timeline<TValue> GetValuesByMinute(int minute) => new Timeline<TValue>(timeline.Where(pair => pair.Time.Minute == minute).ToArray());

        public Timeline<TValue> GetValuesByHour(int hour) => new Timeline<TValue>(timeline.Where(pair => pair.Time.Hour == hour).ToArray());

        public Timeline<TValue> GetValuesByDay(int day) => new Timeline<TValue>(timeline.Where(pair => pair.Time.Day == day).ToArray());

        public Timeline<TValue> GetValuesByTimeOfDay(TimeSpan timeOfDay) => new Timeline<TValue>(timeline.Where(pair => pair.Time.TimeOfDay == timeOfDay).ToArray());

        public Timeline<TValue> GetValuesByDayOfWeek(DayOfWeek dayOfWeek) => new Timeline<TValue>(timeline.Where(pair => pair.Time.DayOfWeek == dayOfWeek).ToArray());

        public Timeline<TValue> GetValuesByDayOfYear(int dayOfYear) => new Timeline<TValue>(timeline.Where(pair => pair.Time.DayOfYear == dayOfYear).ToArray());

        public Timeline<TValue> GetValuesByMonth(int month) => new Timeline<TValue>(timeline.Where(pair => pair.Time.Month == month).ToArray());

        public Timeline<TValue> GetValuesByYear(int year) => new Timeline<TValue>(timeline.Where(pair => pair.Time.Year == year).ToArray());

        public void Add(DateTime time, TValue value)
        {
            timeline.Add((time, value));
            timeline = timeline.OrderBy(pair => pair.Time).ToList();
        }

        public void Add(params (DateTime, TValue)[] timeline)
        {
            this.timeline.AddRange(timeline);
            this.timeline = this.timeline.OrderBy(pair => pair.Time).ToList();
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

        public bool Contains(DateTime time, TValue value) => timeline.Contains((time, value));

        public bool Contains(params (DateTime, TValue)[] timeline) => timeline.All(pair => Contains(pair.Time, pair.Value));

        public bool Contains(Timeline<TValue> timeline) => Contains(timeline.ToArray());

        public bool ContainsTime(params DateTime[] times) => times.All(time => GetAllTimes().Contains(time));

        public bool ContainsValue(params TValue[] values) => values.All(value => GetAllValues().Contains(value));

        public bool Remove(DateTime time, TValue value) => timeline.Remove((time, value));

        public bool Remove(params (DateTime, TValue)[] timeline) => timeline.All(pair => this.timeline.Remove((pair.Time, pair.Value)));

        public bool Remove(Timeline<TValue> timeline) => Remove(timeline.ToArray());

        public bool RemoveTime(params DateTime[] times)
        {
            var result = times.All(time => GetAllTimes().Contains(time));
            if (result)
            {
                timeline = timeline.Where(pair => !times.Contains(pair.Time)).ToList();
            }
            return result;
        }

        public bool RemoveValue(params TValue[] values)
        {
            var result = values.All(value => GetAllValues().Contains(value));
            if (result)
            {
                timeline = timeline.Where(pair => !values.Contains(pair.Value)).ToList();
            }
            return result;
        }

        public (DateTime Time, TValue Value)[] ToArray() => timeline.ToArray();

        public IList<(DateTime Time, TValue Value)> ToList() => timeline;

        public IDictionary<DateTime, TValue> ToDictionary() => timeline.ToDictionary(pair => pair.Time, pair => pair.Value);

        public override bool Equals(object? obj) => obj is Timeline<TValue> timeline && this == timeline;

        public override int GetHashCode() => timeline.GetHashCode();
    }
}