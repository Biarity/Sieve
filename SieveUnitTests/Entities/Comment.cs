using System;
using Sieve.Attributes;

namespace SieveUnitTests.Entities
{
	public class Comment
    {
        public int Id { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;

        [Sieve(CanFilter = true)]
        public string Text { get; set; }
    }
}
