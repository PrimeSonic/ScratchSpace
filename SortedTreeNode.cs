namespace ScratchProjects.FirstPass
{
    using System;
    using System.Collections.Generic;

    internal class SortedTreeNode<IdType, DataType>
        where IdType : IEquatable<IdType>, IComparable<IdType>
        where DataType : ISortable<IdType>
    {
        public SortedTreeNode(IdType id, ISortable<IdType> data, SortedTree<IdType, DataType> tree)
        {
            Id = id;
            Data = data;
            Tree = tree;
        }

        public readonly IdType Id;

        public readonly ISortable<IdType> Data;

        public readonly SortedTree<IdType, DataType> Tree;

        public ErrorTypes Error { get; set; } = ErrorTypes.None;

        public bool HasError => Error != ErrorTypes.None;

        public ICollection<IdType> Dependencies => Data.Dependencies;

        public ICollection<IdType> LoadBeforeRequirements => Data.LoadBeforeRequirements;

        public ICollection<IdType> LoadAfterRequirements => Data.LoadAfterRequirements;

        internal bool HasOrdering => Dependencies.Count > 0 || LoadBeforeRequirements.Count > 0 || LoadAfterRequirements.Count > 0;

        internal bool HasDependencies => Dependencies.Count > 0;

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

        internal bool RequiresBefore(IdType other)
        {
            return LoadBeforeRequirements.Contains(other);
        }

        internal bool RequiresAfter(IdType other)
        {
            return LoadAfterRequirements.Contains(other);
        }

        internal bool DependsOn(IdType other)
        {
            return Dependencies.Contains(other);
        }

        internal static SortResults CompareLoadOrder(SortedTreeNode<IdType, DataType> entity, SortedTreeNode<IdType, DataType> other)
        {
            if (entity.HasError || other.HasError)
            {
                return SortResults.NoSortPreference;
            }

            if (entity.Id.Equals(other.Id))
            {
                entity.Error = ErrorTypes.DuplicateId;
                other.Error = ErrorTypes.DuplicateId;
                return SortResults.DuplicateId;
            }

            bool entityIsDependentOnOther = entity.DependsOn(other.Id);
            bool otherIsDependentOnEntity = other.DependsOn(entity.Id);

            if (entityIsDependentOnOther && otherIsDependentOnEntity)
            {
                entity.Error = ErrorTypes.CircularDependency;
                other.Error = ErrorTypes.CircularDependency;
                return SortResults.CircularDependency;
            }

            if (entityIsDependentOnOther)
            {
                return SortResults.SortBefore;
            }

            if (otherIsDependentOnEntity)
            {
                return SortResults.SortAfter;
            }

            if (entity.RequiresBefore(other.Id) && other.RequiresBefore(entity.Id) ||
                entity.RequiresAfter(other.Id) && other.RequiresAfter(entity.Id))
            {
                entity.Error = ErrorTypes.CircularLoadOrder;
                other.Error = ErrorTypes.CircularLoadOrder;
                return SortResults.CircularLoadOrder;
            }

            if (entity.RequiresBefore(other.Id) || other.RequiresAfter(entity.Id))
            {
                return NextLevelCompareBefore(entity, other);
            }

            if (entity.RequiresAfter(other.Id) || other.RequiresBefore(entity.Id))
            {
                return NextLevelCompareAfter(entity, other);
            }

            var subResultB = SortResults.NoSortPreference;
            var subResultA = SortResults.NoSortPreference;            

            if (entity.LoadBefore != null)
            {
                subResultB = CompareLoadOrder(entity.LoadBefore, other);
            }

            if (entity.LoadAfter != null)
            {
                subResultA = CompareLoadOrder(entity.LoadAfter, other);
            }

            SortResults splitCheckResult = subResultA + (int)subResultB;

            return splitCheckResult <= SortResults.SortAfter 
                ? splitCheckResult 
                : SortResults.CircularLoadOrder;
        }

        private static SortResults NextLevelCompareAfter(SortedTreeNode<IdType, DataType> entity, SortedTreeNode<IdType, DataType> other)
        {
            if (entity.LoadBefore != null)
            {
                SortResults subResult = CompareLoadOrder(entity.LoadBefore, other);

                switch (subResult)
                {
                    case SortResults.SortBefore:
                        other.Error = ErrorTypes.CircularLoadOrder;
                        entity.ChainInCircularLoadOrder();
                        return SortResults.CircularLoadOrder;
                    case SortResults.NoSortPreference:
                        return SortResults.SortAfter;
                    default:
                        return subResult;
                }
            }

            return SortResults.SortAfter;
        }

        private static SortResults NextLevelCompareBefore(SortedTreeNode<IdType, DataType> entity, SortedTreeNode<IdType, DataType> other)
        {
            if (entity.LoadAfter != null)
            {
                SortResults subResult = CompareLoadOrder(entity.LoadAfter, other);

                switch (subResult)
                {
                    case SortResults.SortAfter:
                        other.Error = ErrorTypes.CircularLoadOrder;
                        entity.ChainInCircularLoadOrder();
                        return SortResults.CircularLoadOrder;
                    case SortResults.NoSortPreference:
                        return SortResults.SortBefore;
                    default:
                        return subResult;
                }
            }

            return SortResults.SortBefore;
        }

        public SortResults Sort(SortedTreeNode<IdType, DataType> other, bool testing = false)
        {
            SortResults topLevelResult = CompareLoadOrder(this, other);

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

        protected void ChainInCircularLoadOrder()
        {
            if (this.HasOrdering)
            {
                this.Error = ErrorTypes.CircularLoadOrder;
            }

            this.LoadBefore?.ChainInCircularLoadOrder();
            this.LoadAfter?.ChainInCircularLoadOrder();
        }
    }

}
