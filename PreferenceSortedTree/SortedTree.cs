namespace ScratchProjects.FirstPass
{
    using System;
    using System.Collections.Generic;

    internal class SortedTree<IdType, DataType>
        where IdType : IEquatable<IdType>, IComparable<IdType>
        where DataType : ISortable<IdType>
    {
        private class MetaSortTree
        {
            internal SortedTreeNode<IdType, DataType> Root;
            internal readonly IDictionary<IdType, SortedTreeNode<IdType, DataType>> SortedElements = new Dictionary<IdType, SortedTreeNode<IdType, DataType>>();
            internal int NodesInError;
            internal int NodeCount => SortedElements.Count - NodesInError;
        }

        private readonly MetaSortTree FirstSubTree = new MetaSortTree();
        private readonly MetaSortTree NormalSubTree = new MetaSortTree();
        private readonly MetaSortTree LastSubTree = new MetaSortTree();

        private readonly SortedList<MetaPriority, MetaSortTree> SubTrees;
        private readonly IDictionary<IdType, MetaPriority> KnownKeys = new Dictionary<IdType, MetaPriority>();

        internal int Count => FirstSubTree.NodeCount + NormalSubTree.NodeCount + LastSubTree.NodeCount;
        internal int NodesInError => FirstSubTree.NodesInError + NormalSubTree.NodesInError + LastSubTree.NodesInError;

        public SortedTree()
        {
            SubTrees = new SortedList<MetaPriority, MetaSortTree>(3)
            {
                { MetaPriority.First, FirstSubTree },
                { MetaPriority.Normal, NormalSubTree },
                { MetaPriority.Last, LastSubTree }
            };
        }

        public SortResults Add(ISortable<IdType> data)
        {
            if (IsDuplicateId(data.Id))
            {
                return SortResults.DuplicateId;
            }

            KnownKeys.Add(data.Id, data.MetaSortOrder);

            var entity = new SortedTreeNode<IdType, DataType>(data.Id, data, this);

            MetaSortTree subTree = SubTrees[data.MetaSortOrder];

            if (subTree.Root == null)
            {
                subTree.Root = entity;
                subTree.SortedElements.Add(entity.Id, entity);
                return SortResults.SortAfter;
            }

            SortResults sortResult = subTree.Root.Sort(entity);

            switch (sortResult)
            {
                case SortResults.SortBefore:
                case SortResults.SortAfter:
                    subTree.SortedElements.Add(entity.Id, entity);
                    break;
                default:
                    subTree.NodesInError++;
                    break;
            }

            return sortResult;
        }

        public List<IdType> CreateFlatIndexList()
        {
            var list = new List<IdType>(this.Count);

            foreach (MetaSortTree subTree in SubTrees.Values)
            {
                ClearErrorsCleanTree(subTree);
                CreateFlatIndexList(subTree.Root, list);
            }

            return list;
        }

        private static void CreateFlatIndexList(SortedTreeNode<IdType, DataType> node, ICollection<IdType> list)
        {
            if (node is null)
            {
                return;
            }

            while (true)
            {
                if (node.LoadBefore != null)
                {
                    CreateFlatIndexList(node.LoadBefore, list);
                }

                if (!node.HasError)
                {
                    list.Add(node.Id);
                }

                if (node.LoadAfter != null)
                {
                    node = node.LoadAfter;
                    continue;
                }

                break;
            }
        }

        private bool IsDuplicateId(IdType id)
        {
            if (KnownKeys.ContainsKey(id))
            {
                MetaSortTree subTree = SubTrees[KnownKeys[id]];
                if (subTree.SortedElements.TryGetValue(id, out SortedTreeNode<IdType, DataType> dup))
                {
                    dup.Error = ErrorTypes.DuplicateId;
                    subTree.SortedElements.Remove(id);

                    subTree.NodesInError++;
                    return true;
                }
            }

            return false;
        }

        private bool AllDependenciesArePresent(SortedTreeNode<IdType, DataType> node)
        {
            if (!node.HasDependencies)
            {
                return true;
            }

            bool dependenciesPresent = false;

            foreach (IdType nodeDependency in node.Dependencies)
            {
                foreach (MetaSortTree subTree in SubTrees.Values)
                {
                    if (subTree.SortedElements.ContainsKey(nodeDependency))
                    {
                        dependenciesPresent = true;
                    }
                }
            }

            if (!dependenciesPresent)
            {
                node.Error = ErrorTypes.MissingDepency;
            }

            return dependenciesPresent;
        }

        private void ClearErrorsCleanTree(MetaSortTree subTree)
        {
            var cleanList = new List<SortedTreeNode<IdType, DataType>>();

            foreach (SortedTreeNode<IdType, DataType> entity in subTree.SortedElements.Values)
            {
                if (entity.HasError || !AllDependenciesArePresent(entity))
                {
                    continue;
                }

                entity.ClearLinks();
                cleanList.Add(entity);
                KnownKeys.Remove(entity.Id);
            }

            subTree.SortedElements.Clear();
            subTree.Root = null;
            subTree.NodesInError = 0;

            foreach (SortedTreeNode<IdType, DataType> entity in cleanList)
            {
                Add(entity.Data);
            }
        }
    }

}
