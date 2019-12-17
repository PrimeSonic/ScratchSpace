namespace ScratchProjects.Sorting
{
    using System;
    using System.Collections.Generic;

    public class SortedTree<IdType, DataType>
        where IdType : IEquatable<IdType>, IComparable<IdType>
        where DataType : ISortable<IdType>
    {
        private readonly List<SortedTreeNode<IdType, DataType>> NonSortableNodes = new List<SortedTreeNode<IdType, DataType>>();

        private readonly WeightedList<IdType> KnownDependencies = new WeightedList<IdType>();


        private readonly Dictionary<IdType, SortedTreeNode<IdType, DataType>> KnownNodes = new Dictionary<IdType, SortedTreeNode<IdType, DataType>>();

        public int Count => KnownNodes.Count;

        public DataType this[IdType key] => KnownNodes[key].Data;

        internal int DependencyUsedBy(IdType key)
        {
            return KnownDependencies.GetWeight(key);
        }

        public bool AddSorted(DataType data)
        {
            IdType id = data.Id;

            if (KnownNodes.ContainsKey(id))
                return false;

            

            var node = new SortedTreeNode<IdType, DataType>(id, data, this);

            if (data.Dependencies.Count == 0 &&
                data.LoadBefore.Count == 0 &&
                data.LoadAfter.Count == 0)
            {
                NonSortableNodes.Add(node);
            }
            else
            {
                foreach (IdType dependency in data.Dependencies)
                    KnownDependencies.Add(dependency);
            }
            
            KnownNodes.Add(id, node);

            return true;

            //throw new NotImplementedException();
        }

        public List<KeyValuePair<IdType, DataType>> GetSortedList()
        {
            CleanRedundantDependencies();

            var sortedList = new List<KeyValuePair<IdType, DataType>>(KnownNodes.Count);

            foreach (SortedTreeNode<IdType, DataType> nonSortedNode in NonSortableNodes)
            {
                sortedList.Add(new KeyValuePair<IdType, DataType>(nonSortedNode.Id, nonSortedNode.Data));
            }

            throw new NotImplementedException();
            return sortedList;
        }

        internal SortResults CompareLoadOrder(SortedTreeNode<IdType, DataType> a, SortedTreeNode<IdType, DataType> b)
        {
            throw new NotImplementedException();
        }

        private delegate IList<IdType> GetCollection(SortedTreeNode<IdType, DataType> node);

        private readonly GetCollection GetDependencies = node => node.Dependencies;
        private readonly GetCollection GetLoadBefore = node => node.Dependencies;
        private readonly GetCollection GetLoadAfter = node => node.Dependencies;

        private void CleanupRedundant(IdType ifPresent, IdType removeThis)
        {
            foreach (SortedTreeNode<IdType, DataType> cleanupNode in KnownNodes.Values)
            {
                if (cleanupNode.Dependencies.Contains(ifPresent) && cleanupNode.Dependencies.Contains(removeThis))
                {
                    cleanupNode.Dependencies.Remove(removeThis);
                    KnownDependencies.Remove(removeThis, preserveAtZero: true);
                }
            }
        }

        internal void CleanRedundantDependencies()
        {
            Action cleanupActions = null;
            foreach (SortedTreeNode<IdType, DataType> node in KnownNodes.Values)
            {
                if (node.Dependencies.Count < 2)
                    continue;

                foreach (IdType d1 in node.Dependencies)
                {
                    var nodeD1 = KnownNodes[d1];

                    foreach (IdType d2 in node.Dependencies)
                    {
                        if (d1.Equals(d2))
                            continue;

                        var nodeD2 = KnownNodes[d2];

                        if (HasSortPreferenceWith(nodeD1, GetDependencies, nodeD2))
                        {
                            // d2 can be removed from entries that already have d1
                            IdType d3 = d2;
                            cleanupActions += () => CleanupRedundant(d1, d3);
                        }
                        else if (HasSortPreferenceWith(nodeD2, GetDependencies, nodeD1))
                        {
                            // d1 can be removed from entries that already have d2
                            cleanupActions += () => CleanupRedundant(d2, d1);
                        }
                    }
                }
            }

            cleanupActions?.Invoke();
        }

        private bool HasSortPreferenceWith(SortedTreeNode<IdType, DataType> a, GetCollection getCollection, SortedTreeNode<IdType, DataType> b)
        {
            var collection = getCollection.Invoke(a);
            if (collection.Count == 0)
                return false;

            if (collection.Contains(b.Id))
                return true;

            foreach (IdType aDependency in collection)
            {
                var aDep = KnownNodes[aDependency];

                if (HasSortPreferenceWith(aDep, getCollection, b))
                    return true;
            }

            return false;
        }
    }
}