using System;
using Sieve.Attributes;

namespace SieveUnitTests.Entities
{
	public class BaseEntity
    {
        public int Id { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;
    }
}
