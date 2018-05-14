using System;
using Sieve.Attributes;

namespace SieveTests.Entities
{
	public class Post
    {
        public int Id { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public string Title { get; set; } = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8);

        [Sieve(CanFilter = true, CanSort = true)]
        public int LikeCount { get; set; } = new Random().Next(0, 1000);

        [Sieve(CanFilter = true, CanSort = true)]
        public int CommentCount { get; set; } = new Random().Next(0, 1000);

        [Sieve(CanFilter = true, CanSort = true)]
        public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;
    }
}
