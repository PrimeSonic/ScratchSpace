namespace EasyMarkup
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public static class EasyMarkupExtensions
    {
        private static readonly IDictionary<object, EmCollection> knownCollections = new Dictionary<object, EmCollection>();

        public static string GetEasyMarkupString(this object caller, bool prettyPrint = false)
        {
            if (!TryGetCollection(caller, out EmCollection collection))
            {
                return null;
            }

            if (collection.TryGetFromBackingObject())
            {
                return prettyPrint ? collection.PrettyPrint() : collection.ToString();
            }

            return null;
        }

        public static bool LoadEasyMarkupString(this object caller, string serializedData)
        {
            if (!TryGetCollection(caller, out EmCollection collection))
            {
                return false;
            }

            return collection.FromString(serializedData) && collection.TrySetToBackingObject();
        }

        private static bool TryGetCollection(object caller, out EmCollection collection)
        {
            if (!knownCollections.TryGetValue(caller, out collection))
            {
                collection = new EmCollection(caller);
                knownCollections.Add(caller, collection);
            }

            return true;
        }
    }
}