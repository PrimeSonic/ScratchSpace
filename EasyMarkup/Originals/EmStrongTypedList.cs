﻿namespace EasyMarkup
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Core;

    public class EmStrongTypedList<T> : EmBase, IEnumerable<T>
        where T : IConvertible
    {
        private static HashSet<char> ListDelimeters { get; } = new HashSet<char>
        {
            SpChar_ListItemSplitter,
            SpChar_ValueDelimiter
        };

        public override bool HasValue => this.Values.Count > 0;

        public T this[int index] => this.Values[index];

        public int Count => this.Values.Count;

        public void Add(T item) => this.Values.Add(item);

        public void Clear() => this.Values.Clear();

        public IList<T> Values { get; } = new List<T>();

        public IEnumerator<T> GetEnumerator() => this.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.Values.GetEnumerator();

        public EmStrongTypedList(string key)
        {
            this.Key = key;
        }

        public EmStrongTypedList(string key, IEnumerable<T> values)
            : this(key)
        {
            this.Values = new List<T>();

            foreach (T value in values)
            {
                this.Values.Add(value);
            }
        }

        public override string ToString()
        {
            if (!this.HasValue && this.Optional)
            {
                return string.Empty;
            }

            string val = $"{this.Key}{SpChar_KeyDelimiter}";
            foreach (T value in this.Values)
            {
                val += $"{EscapeSpecialCharacters(value.ToString())}{SpChar_ListItemSplitter}";
            }

            val = val.TrimEnd(SpChar_ListItemSplitter);

            return val + SpChar_ValueDelimiter;
        }

        protected override string ExtractValue(StringBuffer fullString)
        {
            string serialValues = string.Empty;

            do
            {
                string serialValue = ReadUntilDelimiter(fullString, ListDelimeters);

                this.Values.Add(ConvertFromSerial(serialValue));

                serialValues += serialValue + SpChar_ListItemSplitter;
            } while (fullString.Count > 0 && fullString.PeekStart() != SpChar_ValueDelimiter);

            return serialValues.TrimEnd(SpChar_ListItemSplitter);
        }

        internal override EmBase Copy()
        {
            var emTypedList = new EmStrongTypedList<T>(this.Key, this.Values)
            {
                Optional = this.Optional,
            };

            foreach (T value in this.Values)
            {
                emTypedList.Add(value);
            }

            return emTypedList;
        }

        public virtual T ConvertFromSerial(string value)
        {
            Type type = typeof(T);

            if (type.IsEnum)
            {
                return (T)Enum.Parse(type, value);
            }
            else
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
        }

        internal override bool ValueEquals(EmBase other)
        {
            if (other is EmStrongTypedList<T> otherTyped)
            {
                if (this.Count != otherTyped.Count)
                {
                    return false;
                }

                for (int i = 0; i < this.Count; i++)
                {
                    if (!this[i].Equals(otherTyped[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }
}