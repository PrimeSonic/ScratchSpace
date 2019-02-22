namespace ScratchProjects.FirstPass
{
    using System;
    using System.Collections.Generic;

    public interface ISortable<IdType>
        where IdType : IEquatable<IdType>, IComparable<IdType>
    {
        IdType Id { get; }

        ICollection<IdType> Dependencies { get; }
        ICollection<IdType> LoadBeforeRequirements { get; }
        ICollection<IdType> LoadAfterRequirements { get; }

        MetaPriority MetaSortOrder { get; }
    }

}
