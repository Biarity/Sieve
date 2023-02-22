using Sieve.Attributes;
using SieveUnitTests.Abstractions.Strategy;
using SieveUnitTests.Entities;

namespace SieveUnitTests.Abstractions.Entity
{
    public interface IPost: IBaseEntity, IAudit
    {
        [Sieve(CanFilter = true, CanSort = true)]
        string Title { get; set; }
        [Sieve(CanFilter = true, CanSort = true)]
        int LikeCount { get; set; }
        [Sieve(CanFilter = true, CanSort = true)]
        int CommentCount { get; set; }
        [Sieve(CanFilter = true, CanSort = true)]
        int? CategoryId { get; set; }
        [Sieve(CanFilter = true, CanSort = true)]
        bool IsDraft { get; set; }
        string ThisHasNoAttribute { get; set; }
        string ThisHasNoAttributeButIsAccessible { get; set; }
        int OnlySortableViaFluentApi { get; set; }
        Comment TopComment { get; set; }
        Comment FeaturedComment { get; set; }
    }
}
