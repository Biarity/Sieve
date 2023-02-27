using System;
using Sieve.Attributes;
using SieveUnitTests.Abstractions.Entity;

namespace SieveUnitTests.Entities
{
    public class Post : BaseEntity, IPost
    {

        [Sieve(CanFilter = true, CanSort = true)]
        public string Title { get; set; } = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8);

        [Sieve(CanFilter = true, CanSort = true)]
        public int LikeCount { get; set; } = new Random().Next(0, 1000);

        [Sieve(CanFilter = true, CanSort = true)]
        public int CommentCount { get; set; } = new Random().Next(0, 1000);

        [Sieve(CanFilter = true, CanSort = true)]
        public int? CategoryId { get; set; } = new Random().Next(0, 4);

        [Sieve(CanFilter = true, CanSort = true)]
        public bool IsDraft { get; set; }

        public string ThisHasNoAttribute { get; set; }

        public string ThisHasNoAttributeButIsAccessible { get; set; }

        public int OnlySortableViaFluentApi { get; set; }

        public Comment TopComment { get; set; }
        public Comment FeaturedComment { get; set; }
        
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
