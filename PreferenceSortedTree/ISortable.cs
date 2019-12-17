namespace ScratchProjects.Sorting
{
    using System;
    using System.Collections.Generic;

    public interface ISortable<IdType>
        where IdType : IEquatable<IdType>, IComparable<IdType>
    {
        IdType Id { get; }

        IList<IdType> Dependencies { get; }
        IList<IdType> LoadBefore { get; }
        IList<IdType> LoadAfter { get; }
    }

}
