using System;
using Sieve.Attributes;
using SieveUnitTests.Abstractions.Entity;

namespace SieveUnitTests.Entities
{
    public class BaseEntity : IBaseEntity
    {
        [Sieve(CanSort = true)]
        public int Id { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;
    }
}
