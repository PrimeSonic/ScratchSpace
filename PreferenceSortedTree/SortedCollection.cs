﻿namespace ScratchProjects.Sorting
{
    using System;
    using System.Collections.Generic;

    public class SortedCollection<IdType, DataType>
        where IdType : IEquatable<IdType>, IComparable<IdType>
        where DataType : ISortable<IdType>
    {
        private readonly WeightedList<IdType> KnownDependencies = new WeightedList<IdType>();
        private readonly WeightedList<IdType> KnownPreferences = new WeightedList<IdType>();
        private readonly HashSet<IdType> KnownNodes = new HashSet<IdType>();
        private readonly SortedTreeNodeCollection<IdType, DataType> NodesToSort = new SortedTreeNodeCollection<IdType, DataType>();

        public int Count => NodesToSort.Count;

        public DataType this[IdType key] => NodesToSort[key].Data;

        internal int DependencyUsedBy(IdType key)
        {
            return KnownDependencies.GetWeight(key);
        }

        public bool AddSorted(DataType data)
        {
            IdType id = data.Id;

            if (KnownNodes.Contains(id))
            {
                // Duplicate node
                NodesToSort.Remove(id);
                return false;
            }

            LoadSortingPreferences(data);
            KnownNodes.Add(id);
            NodesToSort.Add(id, new SortedTreeNode<IdType, DataType>(id, data, this));

            return true;
        }

        public List<DataType> GetSortedList()
        {
            CleanRedundantDependencies();
            CleanRedundantLoadBefore();
            CleanRedundantLoadAfter();

            SortedTreeNode<IdType, DataType> root = LinkNodes();

            if (root != null)
                AddUnsortedNodes(ref root);

            var sortedList = new List<DataType>(NodesToSort.Count);

            if (root != null)
                AddLinkedNodesToList(ref root, sortedList);

            return sortedList;
        }

        private void AddLinkedNodesToList(ref SortedTreeNode<IdType, DataType> node, List<DataType> list)
        {
            if (node == null)
                return;

            if (node.NodeBefore != null)
                AddLinkedNodesToList(ref node.NodeBefore, list);

            if (node.AllDependenciesPresent(NodesToSort.Keys) && !list.Contains(node.Data))
                list.Add(node.Data);

            if (node.NodeAfter != null)
                AddLinkedNodesToList(ref node.NodeAfter, list);
        }

        private SortedTreeNode<IdType, DataType> LinkNodes()
        {
            List<IdType> orderedDependencies = KnownDependencies.ToSortedList();
            List<IdType> orderedPreferences = KnownPreferences.ToSortedList();

            SortedTreeNode<IdType, DataType> root = null;

            LinkDependencies(orderedDependencies, ref root);

            LinkBeforePreferences(orderedPreferences, ref root);

            LinkAfterPreferences(orderedPreferences, ref root);

            LinkRemaining(ref root);

            return root;
        }

        private void LinkRemaining(ref SortedTreeNode<IdType, DataType> root)
        {
            foreach (SortedTreeNode<IdType, DataType> node in NodesToSort.Values)
            {
                if (!node.IsLinked)
                {
                    if (root == null)
                    {
                        root = node;
                    }
                    else
                    {
                        root.SetNodeBefore(node);
                    }
                }
            }
        }

        private void LinkAfterPreferences(List<IdType> orderedPreferences, ref SortedTreeNode<IdType, DataType> root)
        {
            foreach (IdType depId in orderedPreferences)
            {
                if (NodesToSort.TryGetValue(depId, out SortedTreeNode<IdType, DataType> node))
                {
                    ICollection<SortedTreeNode<IdType, DataType>> dependOn = NodesToSort.FindNodes((n) => n.LoadAfter.Contains(depId) || node.LoadBefore.Contains(n.Id));

                    foreach (SortedTreeNode<IdType, DataType> item in dependOn)
                    {
                        if (!node.IsLinked)
                        {
                            node.SetNodeBefore(item);
                            root = node;
                        }
                    }

                }
            }
        }

        private void LinkBeforePreferences(List<IdType> orderedPreferences, ref SortedTreeNode<IdType, DataType> root)
        {
            foreach (IdType depId in orderedPreferences)
            {
                if (NodesToSort.TryGetValue(depId, out SortedTreeNode<IdType, DataType> node))
                {
                    ICollection<SortedTreeNode<IdType, DataType>> dependOn = NodesToSort.FindNodes((n) => n.LoadBefore.Contains(depId) || node.LoadAfter.Contains(n.Id));

                    foreach (SortedTreeNode<IdType, DataType> item in dependOn)
                    {
                        if (!node.IsLinked)
                        {
                            node.SetNodeAfter(item);
                            root = node;
                        }
                    }
                }
            }
        }

        private void LinkDependencies(List<IdType> orderedDependencies, ref SortedTreeNode<IdType, DataType> root)
        {
            foreach (IdType depId in orderedDependencies)
            {
                if (NodesToSort.TryGetValue(depId, out SortedTreeNode<IdType, DataType> node))
                {
                    ICollection<SortedTreeNode<IdType, DataType>> dependOn = NodesToSort.FindNodes((n) => n.Dependencies.Contains(depId));

                    foreach (SortedTreeNode<IdType, DataType> item in dependOn)
                    {
                        if (!node.IsLinked)
                        {
                            node.SetNodeBefore(item);
                            root = node;
                        }
                    }
                }
            }
        }

        internal List<IdType> GetSortedIndexList()
        {
            List<DataType> list = GetSortedList();

            var indexList = new List<IdType>(list.Count);

            foreach (DataType item in list)
                indexList.Add(item.Id);

            return indexList;
        }

        private void AddUnsortedNodes(ref SortedTreeNode<IdType, DataType> parent)
        {
            // Add nodes without preferences
            foreach (IdType id in NodesToSort.Keys)
            {
                if (NodesToSort.TryGetValue(id, out SortedTreeNode<IdType, DataType> node))
                {
                    if (node.IsLinked)
                        continue;

                    if (!node.HasOrdering)
                    {
                        parent.SetNodeBefore(node);
                        continue;
                    }
                    else if (node.Dependencies.Count == 0)
                    {
                        bool noPreferences = true;
                        foreach (IdType before in node.LoadBefore)
                        {
                            if (NodesToSort.ContainsKey(before))
                            {
                                noPreferences = false;
                                break;
                            }
                        }

                        foreach (IdType after in node.LoadAfter)
                        {
                            if (NodesToSort.ContainsKey(after))
                            {
                                noPreferences = false;
                                break;
                            }
                        }

                        if (noPreferences)
                        {
                            parent.SetNodeAfter(node);
                            continue;
                        }
                    }
                }
            }
        }

        private void LoadSortingPreferences(DataType data)
        {
            foreach (IdType dependency in data.Dependencies)
                KnownDependencies.Add(dependency);

            foreach (IdType before in data.LoadBefore)
                KnownPreferences.Add(before);

            foreach (IdType after in data.LoadAfter)
                KnownPreferences.Add(after);
        }

        private delegate IList<IdType> GetCollection(SortedTreeNode<IdType, DataType> node);
        private readonly GetCollection Dependencies = node => node.Dependencies;
        private readonly GetCollection LoadBefore = node => node.LoadBefore;
        private readonly GetCollection LoadAfter = node => node.LoadAfter;

        private delegate void CleanInternal(IdType id);

        private void CleanupRedundant(IdType ifThisIsPresent, GetCollection inThisCollection, IdType thenRemoveThis, CleanInternal andPassItAlong)
        {
            foreach (SortedTreeNode<IdType, DataType> cleanupNode in NodesToSort.Values)
            {
                IList<IdType> collectionToClean = inThisCollection(cleanupNode);
                if (collectionToClean.Contains(ifThisIsPresent) && collectionToClean.Remove(thenRemoveThis))
                {
                    andPassItAlong.Invoke(thenRemoveThis);
                }
            }
        }

        internal void CleanRedundantDependencies()
        {
            CleanRedundant(Dependencies, CleanDepedencies);
        }

        internal void CleanRedundantLoadBefore()
        {
            CleanRedundant(LoadBefore, CleanPreferences);
        }

        internal void CleanRedundantLoadAfter()
        {
            CleanRedundant(LoadAfter, CleanPreferences);
        }

        private void CleanDepedencies(IdType toRemove)
        {
            KnownDependencies.Remove(toRemove);
        }

        private void CleanPreferences(IdType toRemove)
        {
            KnownPreferences.Remove(toRemove);
        }

        private void CleanRedundant(GetCollection getCollection, CleanInternal cleanInternal)
        {
            Action cleanupActions = null;
            foreach (SortedTreeNode<IdType, DataType> node in NodesToSort.Values)
            {
                IList<IdType> collection = getCollection.Invoke(node);
                if (collection.Count < 2)
                    continue;

                foreach (IdType d1 in collection)
                {
                    SortedTreeNode<IdType, DataType> nodeD1 = NodesToSort[d1];

                    foreach (IdType d2 in collection)
                    {
                        if (d1.Equals(d2))
                            continue;

                        SortedTreeNode<IdType, DataType> nodeD2 = NodesToSort[d2];

                        if (HasSortPreferenceWith(nodeD1, getCollection, nodeD2))
                        {
                            // d2 can be removed from entries that already have d1
                            IdType d3 = d2;
                            cleanupActions += () => CleanupRedundant(d1, getCollection, d3, cleanInternal);
                        }
                        else if (HasSortPreferenceWith(nodeD2, getCollection, nodeD1))
                        {
                            // d1 can be removed from entries that already have d2
                            cleanupActions += () => CleanupRedundant(d2, getCollection, d1, cleanInternal);
                        }
                    }
                }
            }

            cleanupActions?.Invoke();
        }

        private bool HasSortPreferenceWith(SortedTreeNode<IdType, DataType> a, GetCollection getCollection, SortedTreeNode<IdType, DataType> b)
        {
            IList<IdType> collection = getCollection.Invoke(a);
            if (collection.Count == 0)
                return false;

            if (collection.Contains(b.Id))
                return true;

            foreach (IdType aDependency in collection)
            {
                SortedTreeNode<IdType, DataType> aDep = NodesToSort[aDependency];

                if (HasSortPreferenceWith(aDep, getCollection, b))
                    return true;
            }

            return false;
        }
    }
}