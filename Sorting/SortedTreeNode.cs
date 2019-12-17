namespace ScratchProjects.Sorting
{
    using System;
    using System.Collections.Generic;

    internal class SortedTreeNode<IdType, DataType>
        where IdType : IEquatable<IdType>, IComparable<IdType>
        where DataType : ISortable<IdType>
    {
        internal SortedTreeNode(IdType id, DataType data, SortedTree<IdType, DataType> tree)
        {
            Id = id;
            Data = data;
            Tree = tree;
        }

        public readonly IdType Id;

        public readonly DataType Data;

        public readonly SortedTree<IdType, DataType> Tree;

        public ErrorTypes Error { get; set; } = ErrorTypes.None;

        public bool HasError => Error != ErrorTypes.None;

        public IList<IdType> Dependencies => Data.Dependencies;

        public IList<IdType> LoadBeforeRequirements => Data.LoadBefore;

        public IList<IdType> LoadAfterRequirements => Data.LoadAfter;

        internal bool HasOrdering => Dependencies.Count > 0 || LoadBeforeRequirements.Count > 0 || LoadAfterRequirements.Count > 0;

        public int NodesAddedBefore { get; private set; }

        public int NodesAddedAfter { get; private set; }

        public SortedTreeNode<IdType, DataType> Parent { get; protected set; }

        public SortedTreeNode<IdType, DataType> LoadBefore { get; protected set; }
        public SortedTreeNode<IdType, DataType> LoadAfter { get; protected set; }

        public void ClearLinks()
        {
            Parent = null;
            LoadBefore = null;
            LoadAfter = null;
        }

        public SortResults Sort(SortedTreeNode<IdType, DataType> other, bool testing = false)
        {
            SortResults topLevelResult = Tree.CompareLoadOrder(this, other);

            SortResults midLevelResult = SortResults.NoSortPreference;
            switch (topLevelResult)
            {
                case SortResults.DuplicateId:
                case SortResults.CircularDependency:
                case SortResults.CircularLoadOrder:
                    return topLevelResult;
                case SortResults.NoSortPreference:

                    if (LoadBefore != null && LoadAfter != null)
                    {
                        SortResults testAfterResult = SortAfter(other, true);
                        SortResults testBeforeResult = SortBefore(other, true);

                        if (testAfterResult > SortResults.NoSortPreference)
                        {
                            return testAfterResult;
                        }

                        if (testBeforeResult > SortResults.NoSortPreference)
                        {
                            return testBeforeResult;
                        }

                        midLevelResult = testAfterResult > testBeforeResult
                            ? testAfterResult
                            : testBeforeResult;
                    }
                    else if (LoadBefore == null && LoadAfter != null)
                    {
                        SortResults testAfterResult = SortAfter(other, true);

                        if (testAfterResult > SortResults.NoSortPreference)
                        {
                            return testAfterResult;
                        }

                        midLevelResult = testAfterResult;
                    }
                    else if (LoadAfter == null && LoadBefore != null)
                    {
                        SortResults testBeforeResult = SortBefore(other, true);

                        if (testBeforeResult > SortResults.NoSortPreference)
                        {
                            return testBeforeResult;
                        }

                        midLevelResult = testBeforeResult;
                    }

                    if (midLevelResult == SortResults.NoSortPreference)
                    {
                        midLevelResult = SortAfter(other, testing);
                    }

                    break;
                case SortResults.SortBefore:
                    midLevelResult = SortBefore(other, testing);
                    break;
                case SortResults.SortAfter:
                    midLevelResult = SortAfter(other, testing);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (!testing)
            {
                switch (midLevelResult)
                {
                    case SortResults.SortBefore:
                        NodesAddedBefore++;
                        break;
                    case SortResults.SortAfter:
                        NodesAddedAfter++;
                        break;
                }
            }

            return midLevelResult;
        }

        public SortResults SortAfter(SortedTreeNode<IdType, DataType> other, bool testing)
        {
            if (LoadAfter == null)
            {
                if (!testing)
                {
                    LoadAfter = other;
                    other.Parent = this;
                }

                return SortResults.SortAfter;
            }

            return LoadAfter.Sort(other);
        }

        public SortResults SortBefore(SortedTreeNode<IdType, DataType> other, bool testing)
        {
            if (LoadBefore == null)
            {
                if (!testing)
                {
                    LoadBefore = other;
                    other.Parent = this;
                }

                return SortResults.SortBefore;
            }

            return LoadBefore.Sort(other);
        }

    }

}
