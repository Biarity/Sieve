using System;
using Sieve.Attributes;
using SieveUnitTests.ValueObjects;

namespace SieveUnitTests.Entities
{
	public class Comment
    {
        public int Id { get; set; }

        public Name AuthorFirstName { get; set; }

        public Name AuthorLastName { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;

        [Sieve(CanFilter = true)]
        public string Text { get; set; }
    }
}
