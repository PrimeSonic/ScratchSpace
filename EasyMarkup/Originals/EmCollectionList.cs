namespace EasyMarkup
{
    using System.Collections;
    using System.Collections.Generic;
    using Core;

    public class EmCollectionList<ListedType> : EmBase, IEnumerable<EmCollection>
        where ListedType : EmCollection, new()
    {
        public override bool HasValue => Count > 0;

        protected readonly EmCollection Template;

        public EmCollection this[int index] => this.Values[index];

        public int Count => this.Values.Count;

        public void Add(EmCollection item)
        {
            this.Values.Add(item);
        }

        public IList<EmCollection> Values { get; } = new List<EmCollection>();

        public IEnumerator<EmCollection> GetEnumerator() => this.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.Values.GetEnumerator();

        public EmCollectionList(string key)
        {
            this.Key = key;
            Template = new ListedType();
        }

        public override string ToString()
        {
            if (!this.HasValue && this.Optional)
            {
                return string.Empty;
            }

            string val = $"{this.Key}{SpChar_KeyDelimiter}";
            foreach (EmCollection collection in this.Values)
            {
                val += SpChar_BeginComplexValue;

                foreach (EmBase property in collection.Values)
                {
                    if (!property.HasValue && property.Optional)
                    {
                        continue;
                    }

                    val += $"{property}";
                }

                val += $"{SpChar_FinishComplexValue}{SpChar_ListItemSplitter}";
            }

            return val.TrimEnd(SpChar_ListItemSplitter) + SpChar_ValueDelimiter;
        }

        protected override string ExtractValue(StringBuffer fullString)
        {
            string serialValues = $"{SpChar_BeginComplexValue}";

            int openParens = 0;

            var buffer = new StringBuffer();

            do
            {
                switch (fullString.PeekStart())
                {
                    case SpChar_ValueDelimiter when openParens == 0: // End of ComplexList
                    case SpChar_ListItemSplitter when openParens == 0 && fullString.Count > 0: // End of a nested property belonging to this collection
                        fullString.PopFromStart(); // Skip delimiter

                        var collection = (ListedType)Template.Copy();
                        collection.FromString($"{this.Key}{SpChar_KeyDelimiter}{buffer}{SpChar_ValueDelimiter}");
                        this.Values.Add(collection);
                        buffer.Clear();
                        serialValues += $"{collection.SerializedValue}{SpChar_ListItemSplitter}";
                        break;
                    case SpChar_BeginComplexValue:
                        openParens++;
                        goto default;
                    case SpChar_FinishComplexValue:
                        openParens--;
                        if (openParens < 0)
                        {
                            throw new EmException(UnbalancedContainersError, buffer);
                        }

                        goto default;
                    default:
                        buffer.PushToEnd(fullString.PopFromStart());
                        break;
                }
            } while (fullString.Count > 0);

            if (openParens != 0)
            {
                throw new EmException(UnbalancedContainersError, buffer);
            }

            return serialValues.TrimEnd(SpChar_ListItemSplitter) + SpChar_FinishComplexValue;
        }

        internal override EmBase Copy()
        {
            var emCollectionList = new EmCollectionList<ListedType>(this.Key)
            {
                Optional = this.Optional,
            };

            foreach (EmCollection listedType in this.Values)
            {
                emCollectionList.Add((ListedType)listedType.Copy());
            }

            return emCollectionList;
        }

        internal override bool ValueEquals(EmBase other)
        {
            if (other is EmCollectionList<ListedType> otherTyped)
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