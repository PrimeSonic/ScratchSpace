namespace EasyMarkup
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Core;

    public class EmCollection : EmBase
    {
        public class EmPropertyData
        {
            public EmPropertyData(PropertyInfo propertyInfo, EmBase emBase)
            {
                PropertyInfo = propertyInfo;
                EmBase = emBase;
            }

            public readonly PropertyInfo PropertyInfo;
            public readonly EmBase EmBase;
        }

        public EmBase this[string key] => Properties[key].EmBase;

        protected readonly IReadOnlyDictionary<string, EmPropertyData> Properties;

        public IEnumerable<EmBase> Values
        {
            get
            {
                foreach (EmPropertyData data in Properties.Values)
                {
                    yield return data.EmBase;
                }
            }
        }

        public readonly object BackingObject;

        internal EmCollection(object caller, string key = null)
        {
            Type type = caller.GetType();

            if (!type.IsClass)
            {
                throw new ArgumentException("Caller is not a class");
            }

            var emClass = type.GetCustomAttribute<EasyMarkupClass>();

            if (emClass == null)
            {
                throw new ArgumentException($"Caller is not using the {nameof(EasyMarkupClass)} attribute");
            }

            BackingObject = caller;

            PropertyInfo[] properties = type.GetProperties();
            var emProperties = new Dictionary<string, EmPropertyData>(properties.Length);

            foreach (PropertyInfo propertyInfo in properties)
            {
                var emProperty = propertyInfo.GetCustomAttribute<EasyMarkupProperty>();
                if (emProperty != null)
                {
                    var easyMarkupClass = propertyInfo.PropertyType.GetCustomAttribute<EasyMarkupClass>();
                    if (easyMarkupClass != null)
                    {
                        string nestedKey = emProperty.Key ?? propertyInfo.Name;
                        var classProperty = new EmCollection(propertyInfo.GetValue(caller), nestedKey);
                        emProperties.Add(nestedKey, new EmPropertyData(propertyInfo, classProperty));
                    }
                    else
                    {
                        string directKey = emProperty.Key ?? propertyInfo.Name;
                        var valueProperty = new EmProperty(propertyInfo, type, emProperty);
                        emProperties.Add(directKey, new EmPropertyData(propertyInfo, valueProperty));
                    }
                }
            }

            this.Key = key ?? type.Name;
            Properties = emProperties;
        }

        public override string ToString()
        {
            string val = $"{this.Key}{SpChar_KeyDelimiter}{SpChar_BeginComplexValue}";
            foreach (EmPropertyData property in Properties.Values)
            {
                EmBase emBase = property.EmBase;
                if (!emBase.HasValue && emBase.Optional)
                {
                    continue;
                }

                val += $"{emBase}";
            }

            return val + $"{SpChar_FinishComplexValue}{SpChar_ValueDelimiter}";
        }

        public override bool HasValue
        {
            get
            {
                foreach (EmPropertyData property in Properties.Values)
                {
                    if (property.EmBase.HasValue)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        internal override EmBase Copy()
        {
            return new EmCollection(BackingObject, this.Key);
        }

        protected override string ExtractValue(StringBuffer fullString)
        {
            string serialValues = $"{SpChar_BeginComplexValue}";

            int openParens = 0;

            string subKey = null;

            var buffer = new StringBuffer();

            bool exit = false;

            do
            {
                switch (fullString.PeekStart())
                {
                    case SpChar_ValueDelimiter when openParens == 0 && buffer.PeekEnd() == SpChar_FinishComplexValue && fullString.Count == 1: // End of ComplexList
                        exit = true;
                        goto default;
                    case SpChar_ValueDelimiter when openParens == 1 && subKey != null: // End of a nested property belonging to this collection
                        buffer.PushToEnd(fullString.PopFromStart());
                        if (!Properties.ContainsKey(subKey))
                        {
                            Console.WriteLine($"Key Not Found: {subKey} - Current Buffer:{buffer}");
                        }

                        Properties[subKey].EmBase.FromString(buffer.ToString());
                        buffer.Clear();
                        serialValues += Properties[subKey].ToString();
                        subKey = null;
                        goto default;
                    case SpChar_KeyDelimiter when openParens == 1: // Key to a nested property belonging to this collection
                        buffer.PopFromStartIfEquals(SpChar_BeginComplexValue);
                        buffer.PopFromStartIfEquals(SpChar_ListItemSplitter);
                        subKey = buffer.ToString();
                        goto default;
                    case SpChar_EscapeChar:
                        buffer.PushToEnd(fullString.PopFromStart()); // Include escape char to be handled by EmProperty
                        goto default;
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
            } while (fullString.Count > 0 && !exit);

            if (openParens != 1)
            {
                throw new EmException(UnbalancedContainersError, buffer);
            }

            return serialValues + SpChar_FinishComplexValue;
        }

        internal override bool ValueEquals(EmBase other)
        {
            if (other is EmCollection otherTyped)
            {
                if (Properties.Count != otherTyped.Properties.Count)
                {
                    return false;
                }

                foreach (KeyValuePair<string, EmPropertyData> property in Properties)
                {
                    if (!otherTyped.Properties.ContainsKey(property.Key))
                    {
                        return false;
                    }

                    if (!property.Value.EmBase.Equals(otherTyped[property.Key]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        internal bool TryGetFromBackingObject()
        {
            bool valid = true;
            foreach (EmPropertyData pair in Properties.Values)
            {
                switch (pair.EmBase)
                {
                    case EmProperty direct:
                        valid &= direct.TryGetCallerValue(BackingObject);
                        break;
                    case EmCollection nested:
                        valid &= nested.TryGetFromBackingObject();
                        break;
                }

                if (!valid)
                    return false;
            }

            return true;
        }

        internal bool TrySetToBackingObject()
        {
            bool valid = true;
            foreach (EmPropertyData pair in Properties.Values)
            {
                switch (pair.EmBase)
                {
                    case EmProperty direct:
                        valid &= direct.TrySetCallerValue(BackingObject);
                        break;
                    case EmCollection nested:
                        valid &= nested.TrySetToBackingObject();
                        break;
                }

                if (!valid)
                    return false;
            }

            return true;
        }
    }
}